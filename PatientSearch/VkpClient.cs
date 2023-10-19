using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using OneOf;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using Claim = System.Security.Claims.Claim;

namespace PatientSearch
{
    /// <summary>
    /// Represents an example client used for api-communication.
    /// </summary>
    internal class VkpClient
    {
        private readonly HelseIdConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private DateTime? _persistedAccessTokenExpiresAt;
        private string? _persistedAccessToken;
        private int _tokenExpirationSkew = 30;

        private DiscoveryDocumentResponse? _disco;

        /// <summary>
        /// Creates an instance of the client.
        /// </summary>
        /// <param name="configuration">Configuration parameters.</param>
        /// <param name="httpClient">The HttpClient used for communication.</param>
        public VkpClient(HelseIdConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<OneOf<Bundle, OperationOutcome>> PatientSearchAsync(string orgNo, string? identifier = null)
        {
            // 1. Get token.
            var token = await GetBearerTokenAsync();

            // 2. Setup call to patient search.
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"https://api.test.vkpnorge.no/epj/r4/{orgNo}/patient/_search");
            // 3. Add bearer token acquired previously.
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // 4. Specify required service codes.
            httpRequest.Headers.Add("ServiceCodes", "rt");
            // 5. (Optional) Specify patient
            if (identifier != null)
            {
                httpRequest.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("identifier", identifier)
                });
            }

            var response = await _httpClient.SendAsync(httpRequest);
            var json = await response.Content.ReadAsStringAsync();
            var parser = new FhirJsonParser();

            // Evaluate the response
            switch (response)
            {
                case { StatusCode: HttpStatusCode.BadRequest }:
                    return parser.Parse<OperationOutcome>(json);
                default:
                    response.EnsureSuccessStatusCode();
                    return parser.Parse<Bundle>(json);
            }
        }

        private async Task<DiscoveryDocumentResponse> GetDiscoveryDocumentAsync()
        {
            var disco = await _httpClient.GetDiscoveryDocumentAsync(_configuration.Authority);
            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            return _disco = disco;
        }

        private async Task<string> GetBearerTokenAsync()
        {
            if (DateTime.UtcNow < _persistedAccessTokenExpiresAt)
            {
                return _persistedAccessToken!;
            }

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
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
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