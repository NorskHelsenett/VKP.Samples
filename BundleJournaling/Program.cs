using Common;


var client = VkpClient.Create();

if (client == null)
{
    Console.WriteLine("Unable to create VkpClient!");
    return;
}

Console.WriteLine($"Journaling Bundle ...");
var result = await client.BundleJournaling("Bundle_OppfølgingAvVarsel_TrygghetsAlarm.json");

result.Switch(
    _ =>
    {
        Console.WriteLine($"Success!");
    },
    error =>
    {
        Console.WriteLine("OperationOutcome received from the API.");

        foreach (var problem in error.Issue)
        {
            Console.WriteLine(problem.Details.Text);
        }
    });
