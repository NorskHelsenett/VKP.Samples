using Common;
using HelseId.Library;
using HelseId.Library.ClientCredentials;
using HelseId.Library.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PatientSearch
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var helseIdConfiguration = new HelseIdConfiguration
            {
                ClientId = ConfigurationValues.ClientCredentialsId,
                Scope = "nhn:vkp/api/user/patient.read",
                IssuerUri = ConfigurationValues.IssuerUri,
            };

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddHelseIdClientCredentials(helseIdConfiguration)
                .AddJwkForClientAuthentication(ConfigurationValues.ClientCredentialsSampleRsaPrivateKeyJwk);

            serviceCollection.AddSingleton<HelseIdService>();
            serviceCollection.AddSingleton<VkpService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var vkpService = serviceProvider.GetService<VkpService>();

            Console.WriteLine($"{Environment.NewLine}PatientSearch without identifier ...");

            var result = await vkpService!.PatientSearchAsync();

            result.Switch(
                bundle => { Console.WriteLine($"Bundle with {bundle.Entry.Count} entries received from the API."); },
                error =>
                {
                    Console.WriteLine("OperationOutcome received from the API.");

                    if (error.Issue.Count > 0)
                    {
                        Console.WriteLine(error.Issue[0].Diagnostics);
                    }
                });


            const string identifier = Constants.TestPatientNin;

            Console.WriteLine($"PatientSearch with identifier '{identifier}' ...");

            var result2 = await vkpService!.PatientSearchAsync(identifier);

            result2.Switch(
                bundle => { Console.WriteLine($"Bundle with {bundle.Entry.Count} entries received from the API."); },
                error =>
                {
                    Console.WriteLine("OperationOutcome received from the API.");

                    if (error.Issue.Count > 0)
                    {
                        Console.WriteLine(error.Issue[0].Diagnostics);
                    }
                });
        }
    }
}