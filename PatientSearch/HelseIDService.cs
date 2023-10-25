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
        private const int TokenExpirationSkew = 30;

        private readonly HelseIdConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private DateTime? _persistedAccessTokenExpiresAt;
        private string? _persistedAccessToken;
        private DiscoveryDocumentResponse? _disco;

        /// <summary>
        /// Creates an instance of the HelseIdService class.
        /// </summary>
        /// <param name="configuration">HelseID configuration used for acquiring bearer tokens.</param>
        /// <param name="httpClient">HttpClient used for communication with the HelseID services.</param>
        public HelseIdService(HelseIdConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get the discovery document from the Authority.
        /// Should be cached to prevent unnecessary calls to the HelseID services.
        /// </summary>
        /// <returns>The discovery document.</returns>
        /// <exception cref="Exception">Thrown when there's an error fetching the discovery document.</exception>
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
        /// Get a bearer token from the HelseID services.
        /// </summary>
        /// <returns>A bearer token.</returns>
        /// <exception cref="Exception">Thrown when there's an error fetching the bearer token or if the access token is missing.</exception>
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
                Scope = string.Join(" ", _configuration.Scopes),
                ClientAssertion = new ClientAssertion
                {
                    Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                    Value = BuildClientAssertion(disco, _configuration.ClientId, _configuration.PrivateJwk, _configuration.Algorithm)
                },
                ClientCredentialStyle = ClientCredentialStyle.PostBody
            });

            if (response.IsError)
            {
                throw new Exception(response.Error);
            }

            if (string.IsNullOrWhiteSpace(response.AccessToken))
            {
                throw new Exception("Access token is missing.");
            }

            // Store the access token for subsequent calls to the API.
            _persistedAccessToken = response.AccessToken;
            _persistedAccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn - TokenExpirationSkew);

            return response.AccessToken;
        }

        /// <summary>
        /// Builds a client assertion using provided parameters.
        /// </summary>
        /// <param name="disco">The discovery document.</param>
        /// <param name="clientId">Client ID.</param>
        /// <param name="jwkPrivateKey">JWK private key.</param>
        /// <param name="algorithm">Signing algorithm.</param>
        /// <returns>The client assertion token string.</returns>
        private static string BuildClientAssertion(DiscoveryDocumentResponse disco, string clientId, string jwkPrivateKey, string algorithm)
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
                DateTime.UtcNow.AddSeconds(30),
                GetClientAssertionSigningCredentials(jwkPrivateKey, algorithm));

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(credentials);
        }

        /// <summary>
        /// Generates signing credentials using provided JWK private key and algorithm.
        /// </summary>
        /// <param name="jwkPrivateKey">JWK private key.</param>
        /// <param name="algorithm">Signing algorithm.</param>
        /// <returns>The signing credentials.</returns>
        private static SigningCredentials GetClientAssertionSigningCredentials(string jwkPrivateKey, string algorithm)
        {
            var securityKey = new JsonWebKey(jwkPrivateKey);
            return new SigningCredentials(securityKey, algorithm);
        }
    }
}