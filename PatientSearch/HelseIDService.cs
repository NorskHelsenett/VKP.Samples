using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PatientSearch
{
    /// <summary>
    /// Represents a service for communicating with the HelseID services.
    /// </summary>
    internal class HelseIdService
    {
        private readonly HelseIdConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private DateTime? _persistedAccessTokenExpiresAt;
        private string? _persistedAccessToken;
        private int _tokenExpirationSkew = 30;

        private DiscoveryDocumentResponse? _disco;

        /// <summary>
        /// Creates an instance of the HelseIdService class.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="httpClient"></param>
        public HelseIdService(HelseIdConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get the discovery document from the Authority.
        /// </summary>
        /// <returns>The discovery document.</returns>
        /// <exception cref="Exception"></exception>
        private async Task<DiscoveryDocumentResponse> GetDiscoveryDocumentAsync()
        {
            var disco = await _httpClient.GetDiscoveryDocumentAsync(_configuration.Authority);
            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            // Save a cached instance of the discovery document.
            return _disco = disco;
        }

        /// <summary>
        /// Get a bearer token from the Authority.
        /// </summary>
        /// <returns>A bearer token.</returns>
        /// <exception cref="Exception"></exception>
        internal async Task<string> GetBearerTokenAsync()
        {
            if (DateTime.UtcNow < _persistedAccessTokenExpiresAt)
            {
                return _persistedAccessToken!;
            }

            // Use the cached discovery document or fetch a new one.
            var disco = _disco ?? await GetDiscoveryDocumentAsync();

            var response = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _configuration.ClientId,
                GrantType = OidcConstants.GrantTypes.ClientCredentials,
                Scope = string.Join(" ", _configuration.Scopes),
                ClientAssertion = new ClientAssertion
                {
                    Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                    Value = BuildClientAssertion(disco, _configuration.ClientId, _configuration.PrivateJwk)
                },
                ClientCredentialStyle = ClientCredentialStyle.PostBody
            });

            if (response.IsError)
            {
                throw new Exception(response.Error);
            }

            _persistedAccessToken = response.AccessToken;
            _persistedAccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn - _tokenExpirationSkew);

            return response.AccessToken ?? throw new Exception("Access token is missing.");
        }

        private static string BuildClientAssertion(DiscoveryDocumentResponse disco, string clientId, string jwkPrivateKey)
        {
            var claims = new List<Claim>
            {
                new(JwtClaimTypes.Subject, clientId),
                new(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                new(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
            };
            var credentials = new JwtSecurityToken(
                clientId,
                disco.TokenEndpoint,
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddSeconds(60),
                GetClientAssertionSigningCredentials(jwkPrivateKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(credentials);
        }

        private static SigningCredentials GetClientAssertionSigningCredentials(string jwkPrivateKey)
        {
            var securityKey = new JsonWebKey(jwkPrivateKey);
            return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        }
    }
}