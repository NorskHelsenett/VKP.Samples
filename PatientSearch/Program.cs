using Common;


var client = VkpClient.Create();

if (client == null)
{
    Console.WriteLine("Unable to create VkpClient!");
    return;
}

const string identifier = "13116900216";

Console.WriteLine($"PatientSearch with identifier '{identifier}' ...");
var singleResult = await client.PatientSearchAsync(identifier);

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

Console.WriteLine($"{Environment.NewLine}PatientSearch without identifier ...");
var result = await client.PatientSearchAsync();


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
