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
                ClientId = ConfigurationValues.ClientCredentialsSampleClientId,
                Scope = "",
                IssuerUri = ConfigurationValues.IssuerUri,
            };

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddHelseIdClientCredentials(helseIdConfiguration)
                .AddJwkForClientAuthentication(ConfigurationValues.ClientCredentialsSampleRsaPrivateKeyJwk);

            serviceCollection.AddSingleton<HelseIdServiceV2>();
            serviceCollection.AddSingleton<VkpService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var serviceForTokenAndApiResponses = serviceProvider.GetService<VkpService>();

            await serviceForTokenAndApiResponses!.EpisodeOfCareSearchAsync();
        }
    }
}