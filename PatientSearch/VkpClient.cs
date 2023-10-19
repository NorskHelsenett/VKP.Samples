using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using OneOf;
using System.Net;
using System.Net.Http.Headers;

namespace PatientSearch
{
    /// <summary>
    /// Represents an example client used for api-communication.
    /// </summary>
    internal class VkpClient
    {
        private readonly HelseIdService _helseIdService;
        private readonly HttpClient _httpClient;


        /// <summary>
        /// Creates an instance of the VkpClient class.
        /// </summary>
        /// <param name="helseIdService">The HelseID service used for acquiring tokens.</param>
        /// <param name="httpClient">The HttpClient used for communication.</param>
        public VkpClient(HelseIdService helseIdService, HttpClient httpClient)
        {
            _helseIdService = helseIdService;
            _httpClient = httpClient;
        }

        public async Task<OneOf<Bundle, OperationOutcome>> PatientSearchAsync(string orgNo, string? identifier = null)
        {
            // 1. Get token.
            var token = await _helseIdService.GetBearerTokenAsync();

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
    }
}