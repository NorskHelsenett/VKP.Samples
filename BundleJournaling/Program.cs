using Common;
using HelseId.Library;
using HelseId.Library.ClientCredentials;
using HelseId.Library.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BundleJournaling
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var helseIdConfiguration = new HelseIdConfiguration
            {
                ClientId = ConfigurationValues.ClientCredentialsId,
                Scope = "nhn:vkp/api/user/bundle.write",
                IssuerUri = ConfigurationValues.IssuerUri,
            };

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddHelseIdClientCredentials(helseIdConfiguration)
                .AddJwkForClientAuthentication(ConfigurationValues.ClientCredentialsSampleRsaPrivateKeyJwk);

            serviceCollection.AddSingleton<HelseIdService>();
            serviceCollection.AddSingleton<VkpService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var vkpService = serviceProvider.GetService<VkpService>();


            Console.WriteLine($"Journaling Bundle ...");

            var result = await vkpService!.BundleJournaling("Bundle_OppfølgingAvVarsel_TrygghetsAlarm.json");

            result.Switch(
                _ => { Console.WriteLine($"Success!"); },
                error =>
                {
                    Console.WriteLine("OperationOutcome received from the API.");

                    foreach (var problem in error.Issue)
                    {
                        Console.WriteLine(problem.Details.Text);
                    }
                });
        }
    }
}