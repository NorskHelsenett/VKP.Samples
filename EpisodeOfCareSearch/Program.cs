using Common;
using HelseId.Library;
using HelseId.Library.ClientCredentials;
using HelseId.Library.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EpisodeOfCareSearch
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var helseIdConfiguration = new HelseIdConfiguration
            {
                ClientId = ConfigurationValues.ClientCredentialsId,
                Scope = "nhn:vkp/api/user/episodeofcare.read",
                IssuerUri = ConfigurationValues.IssuerUri,
            };

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddHelseIdClientCredentials(helseIdConfiguration)
                .AddJwkForClientAuthentication(ConfigurationValues.ClientCredentialsSampleRsaPrivateKeyJwk);

            serviceCollection.AddSingleton<HelseIdService>();
            serviceCollection.AddSingleton<VkpService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var vkpService = serviceProvider.GetService<VkpService>();

            var result = await vkpService!.EpisodeOfCareSearchAsync();

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

            var result2 = await vkpService!.EpisodeOfCareSearchAsync(identifier);

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