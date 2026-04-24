using HelseId.Library.ClientCredentials.Interfaces;
using HelseId.Library.ExtensionMethods;

namespace Common
{
    /// <summary>
    /// Represents a service for communicating with the HelseID services.
    /// </summary>
    public class HelseIdService
    {
        private readonly IHelseIdClientCredentialsFlow _helseIdClientCredentialsFlow;

        /// <summary>
        /// Creates an instance of the HelseIdService class.
        /// </summary>
        /// <param name="helseIdClientCredentialsFlow"></param>
        public HelseIdService(IHelseIdClientCredentialsFlow helseIdClientCredentialsFlow)
        {
            _helseIdClientCredentialsFlow = helseIdClientCredentialsFlow;
        }


        /// <summary>
        /// Get a DPoP token from the HelseID services.
        /// </summary>
        /// <returns>A bearer token.</returns>
        /// <exception cref="Exception">Thrown when there's an error fetching the bearer token or if the access token is missing.</exception>
        internal async Task<HelseId.Library.Models.AccessTokenResponse> GetTokenAsync()
        {
            var tokenResponse = await _helseIdClientCredentialsFlow.GetTokenResponseAsync();

            // Checks if the access token response is successful  
            if (!tokenResponse.IsSuccessful(out var accessTokenResponse))
            {
                // If the response is not successful, you can get an error response:
                var errorResponse = tokenResponse.AsError();
                throw new Exception(errorResponse.Error + " " + errorResponse.ErrorDescription);
            }

            if (string.IsNullOrWhiteSpace(accessTokenResponse.AccessToken))
            {
                throw new Exception("Access token is missing.");
            }

            return accessTokenResponse;
        }
    }
}