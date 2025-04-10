using Common;


var client = VkpClient.Create();

if (client == null)
{
    Console.WriteLine("Unable to create VkpClient!");
    return;
}

const string identifier = Constants.TestPatientNin;

Console.WriteLine($"ConditionSearch with identifier '{identifier}' ...");
var result = await client.ConditionSearchAsync(identifier);

result.Switch(
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
