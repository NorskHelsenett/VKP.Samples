using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using OneOf;
using System.Net;
using System.Net.Http.Headers;

namespace Common;

/// <summary>
/// Represents an example client used for api-communication.
/// </summary>
public class VkpClient
{
    private readonly HelseIdService _helseIdService;
    private readonly HttpClient _httpClient;


    /// <summary>
    /// Creates an instance of the VkpClient class.
    /// </summary>
    /// <param name="helseIdService">The HelseID service used for acquiring tokens.</param>
    /// <param name="httpClient">The HttpClient used for communication.</param>
    private VkpClient(HelseIdService helseIdService, HttpClient httpClient)
    {
        _helseIdService = helseIdService;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Creates an instance of the VkpClient class.
    /// </summary>
    public static VkpClient? Create()
    {
        
        var helseIdConfig = HelseIdConfiguration.ReadFromFile(Constants.HelseIdConfigFilename);

        if (helseIdConfig == null)
        {
            Console.WriteLine("Missing HelseID configuration.");
            return null;
        }

        var httpClient = new HttpClient();
        var service = new HelseIdService(helseIdConfig, httpClient);

        return new VkpClient(service, httpClient);
    }

    /// <summary>
    /// Sends request to VKP's PatientSearch endpoint.
    /// </summary>
    /// <param name="identifier">Optional patient identifier</param>
    /// <returns>Bundle with patient(s), or OperationOutcome in case of error.</returns>
    public async Task<OneOf<Bundle, OperationOutcome>> PatientSearchAsync(string? identifier = null)
    {
        var httpRequest = await CreateHttpRequestMessageAsync("patient/_search");

        // Specify required service codes.
        httpRequest.Headers.Add("ServiceCodes", "rt");

        // (Optional) Specify patient
        if (identifier != null)
        {
            httpRequest.Content = new FormUrlEncodedContent(
            [ new KeyValuePair<string, string>("identifier", identifier) ]);
        }

        return await SendSearchRequestAsync(httpRequest);
    }

    /// <summary>
    /// Sends request to VKP's AllergyIntoleranceSearch endpoint.
    /// </summary>
    /// <param name="identifier">Optional patient identifier</param>
    /// <returns>Bundle with patient(s), or OperationOutcome in case of error.</returns>
    public async Task<OneOf<Bundle, OperationOutcome>> AllergyIntoleranceSearchAsync(string identifier)
    {
        var httpRequest = await CreateHttpRequestMessageAsync("allergyintolerance/_search");

        // Specify required service codes.
        httpRequest.Headers.Add("ServiceCodes", "ta");

        // Specify patient
        httpRequest.Content = new FormUrlEncodedContent(
                [ new KeyValuePair<string, string>("identifier", identifier) ]);

        return await SendSearchRequestAsync(httpRequest);
    }

    /// <summary>
    /// Creates HTTP request object, including header for the retrieved bearer token.
    /// </summary>
    /// <param name="apiEndpoint">Last part of the API endpoint</param>
    /// <returns>HttpRequestMessage</returns>
    private async Task<HttpRequestMessage> CreateHttpRequestMessageAsync(string apiEndpoint)
    {
        // Create request to API endpoint
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{Constants.ApiBaseUrl}/{Constants.TestOrgNumber}/{apiEndpoint}");

        // Get token and update request
        var token = await _helseIdService.GetBearerTokenAsync();
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return httpRequest;
    }

    /// <summary>
    /// Sends request and returns the result (assuming request to one of the search endpoints).
    /// </summary>
    /// <param name="request">HttpRequestMessage object, with all headers (and body) set.</param>
    /// <returns></returns>
    private async Task<OneOf<Bundle, OperationOutcome>> SendSearchRequestAsync(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
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
