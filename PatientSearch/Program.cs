using PatientSearch;

// Read HelseID configuration from file.
// An example file is included. Only the privateJwk and authority is used.
var helseIdConfiguration = HelseIdConfiguration.ReadFromFile("HelseID.json");

if (helseIdConfiguration == null)
{
    Console.WriteLine("Missing configuration.");
    return;
}

var httpClient = new HttpClient();
var helseIdService = new HelseIdService(helseIdConfiguration, httpClient);
var client = new VkpClient(helseIdService, httpClient);

var result = await client.PatientSearchAsync("888134576", "13116900216");

result.Switch(bundle =>
    {
        Console.WriteLine($"Bundle with {bundle.Entry.Count} entries received from the API.");
    },
    outcome =>
    {
        Console.WriteLine("OperationOutcome received from the API.");
    });
