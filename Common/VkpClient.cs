﻿using Hl7.Fhir.Model;
using Hl7.Fhir.Model.CdsHooks;
using Hl7.Fhir.Serialization;
using OneOf;
using OneOf.Types;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
    /// Sends journaling request to VKP's bundle endpoint.
    /// </summary>
    /// <param name="messageFilename"></param>
    /// <returns>Success, or OperationOutcome in case of error.</returns>
    public async Task<OneOf<Success, OperationOutcome>> BundleJournaling(string messageFilename)
    {
        var message = await ReadMessageFileAsync(messageFilename);

        var httpRequest = await CreateHttpRequestMessageAsync("bundle");
        httpRequest.Content = new StringContent(message);

        return await SendJournalingRequestAsync(httpRequest);
    }

    /// <summary>
    /// Sends request to VKP's AllergyIntoleranceSearch endpoint.
    /// </summary>
    /// <param name="identifier">Patient identifier</param>
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
    /// Sends request to VKP's CarePlanSearch endpoint.
    /// </summary>
    /// <param name="identifier">Patient identifier</param>
    /// <returns>Bundle with patient(s), or OperationOutcome in case of error.</returns>
    public async Task<OneOf<Bundle, OperationOutcome>> CarePlanSearchAsync(string identifier)
    {
        var httpRequest = await CreateHttpRequestMessageAsync("careplan/_search");

        // Specify required service codes.
        httpRequest.Headers.Add("ServiceCodes", "ta,gps");

        // Specify patient
        httpRequest.Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("patient.identifier", identifier),
                new KeyValuePair<string, string>("date", "ge2024-05-17T07:08:09Z"),
                new KeyValuePair<string, string>("date", "le2024-12-24T09:08:07Z"),
            ]);

        return await SendSearchRequestAsync(httpRequest);
    }

    /// <summary>
    /// Sends request to VKP's CompositionSearch endpoint.
    /// </summary>
    /// <param name="identifier">Patient identifier</param>
    /// <returns>Bundle with patient(s), or OperationOutcome in case of error.</returns>
    public async Task<OneOf<Bundle, OperationOutcome>> CompositionSearchAsync(string identifier)
    {
        var httpRequest = await CreateHttpRequestMessageAsync("composition/_search");

        // Specify required service codes.
        httpRequest.Headers.Add("ServiceCodes", "dho");

        // Specify patient
        httpRequest.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("patient.identifier", identifier),
            new KeyValuePair<string, string>("date", "ge2024-05-17T07:08:09Z"),
            new KeyValuePair<string, string>("date", "le2024-12-24T09:08:07Z"),
        ]);

        return await SendSearchRequestAsync(httpRequest);
    }

    /// <summary>
    /// Sends request to VKP's ConditionSearch endpoint.
    /// </summary>
    /// <param name="identifier">Patient identifier</param>
    /// <returns>Bundle with patient(s), or OperationOutcome in case of error.</returns>
    public async Task<OneOf<Bundle, OperationOutcome>> ConditionSearchAsync(string identifier)
    {
        var httpRequest = await CreateHttpRequestMessageAsync("condition/_search");

        // Specify required service codes.
        httpRequest.Headers.Add("ServiceCodes", "ta");

        // Specify patient
        httpRequest.Content = new FormUrlEncodedContent(
        [ new KeyValuePair<string, string>("patient.identifier", identifier) ]);

        return await SendSearchRequestAsync(httpRequest);
    }

    /// <summary>
    /// Sends request to VKP's EpisodeOfCareSearch endpoint.
    /// </summary>
    /// <param name="identifier">Optional patient identifier</param>
    /// <returns>Bundle with patient(s), or OperationOutcome in case of error.</returns>
    public async Task<OneOf<Bundle, OperationOutcome>> EpisodeOfCareSearchAsync(string? identifier = null)
    {
        var httpRequest = await CreateHttpRequestMessageAsync("episodeofcare/_search");

        // Specify required service codes.
        httpRequest.Headers.Add("ServiceCodes", "rt");

        // (Optional) Specify patient
        if (identifier != null)
        {
            httpRequest.Content = new FormUrlEncodedContent(
                [new KeyValuePair<string, string>("patient.identifier", identifier)]);
        }

        return await SendSearchRequestAsync(httpRequest);
    }

    /// <summary>
    /// Sends request to VKP's MedicationStatementSearch endpoint.
    /// </summary>
    /// <param name="identifier">Patient identifier</param>
    /// <returns>Bundle with patient(s), or OperationOutcome in case of error.</returns>
    public async Task<OneOf<Bundle, OperationOutcome>> MedicationStatementSearchAsync(string identifier)
    {
        var httpRequest = await CreateHttpRequestMessageAsync("medicationstatement/_search");

        // Specify required service codes.
        httpRequest.Headers.Add("ServiceCodes", "ta");

        // Specify patient
        httpRequest.Content = new FormUrlEncodedContent(
            [new KeyValuePair<string, string>("patient.identifier", identifier)]);

        return await SendSearchRequestAsync(httpRequest);
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
                [new KeyValuePair<string, string>("identifier", identifier)]);
        }

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

    /// <summary>
    /// Sends request and returns the result (assuming request to one of the journaling endpoints).
    /// </summary>
    /// <param name="request">HttpRequestMessage object, with headers and body set.</param>
    /// <returns></returns>
    private async Task<OneOf<Success, OperationOutcome>> SendJournalingRequestAsync(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var json = await response.Content.ReadAsStringAsync();
            var parser = new FhirJsonParser();
            return parser.Parse<OperationOutcome>(json);
        }

        response.EnsureSuccessStatusCode();
        return new Success();
    }


    /// <summary>
    /// Reads message from file
    /// </summary>
    /// <param name="filename"></param>
    /// <returns>File contents as string</returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    private static async Task<string> ReadMessageFileAsync(string filename)
    {
        var relativePath = $"Messages/{filename}";

        if (string.IsNullOrEmpty(relativePath) || !File.Exists(relativePath))
        {
            throw new FileNotFoundException($"The specified file does not exist: {relativePath}");
        }

        var message = await File.ReadAllTextAsync(relativePath);

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new FileLoadException("Unable to read file contents");
        }

        return message;
    }
}
