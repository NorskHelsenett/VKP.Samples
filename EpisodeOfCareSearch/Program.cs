using Common;


var client = VkpClient.Create();

if (client == null)
{
    Console.WriteLine("Unable to create VkpClient!");
    return;
}

const string identifier = Constants.TestPatientNin;

Console.WriteLine($"EpisodeOfCareSearch with identifier '{identifier}' ...");
var singleResult = await client.EpisodeOfCareSearchAsync(identifier);

singleResult.Switch(
    bundle =>
    {
        Console.WriteLine($"Bundle with {bundle.Entry.Count} entries received from the API.");
    },
    error =>
    {
        Console.WriteLine("OperationOutcome received from the API.");

        if (error.Issue.Count > 0)
        {
            Console.WriteLine(error.Issue[0].Diagnostics);
        }
    });

Console.WriteLine($"{Environment.NewLine}EpisodeOfCareSearch without identifier ...");
var result = await client.EpisodeOfCareSearchAsync();


result.Switch(
    bundle =>
    {
        Console.WriteLine($"Bundle with {bundle.Entry.Count} entries received from the API (identifier = null).");
    },
    error =>
    {
        Console.WriteLine("OperationOutcome received from the API.");

        if (error.Issue.Count > 0)
        {
            Console.WriteLine(error.Issue[0].Diagnostics);
        }
    });
