using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace PatientSearch
{
    /// <summary>
    /// Internal class representing the HelseID configuration.
    /// </summary>
    internal class HelseIdConfiguration
    {
        public string ClientName { get; set; } = null!;
        public string Authority { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string[] GrantTypes { get; set; } = [];
        public string[] Scopes { get; set; } = [];
        public string SecretType { get; set; } = null!;
        public string RsaPrivateKey { get; set; } = null!;
        public int RsaKeySizeBits { get; set; }
        public string PrivateJwk { get; set; } = null!;

        /// <summary>
        /// Algorithm used for signing the token.
        /// Is not included in the HelseID configuration.
        /// </summary>
        public string Algorithm { get; set; } = SecurityAlgorithms.RsaSha256;

        public static HelseIdConfiguration? ReadFromFile(string filename)
        {
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file does not exist.", filename);
            }

            var jsonContent = File.ReadAllText(filename);

            return JsonSerializer.Deserialize<HelseIdConfiguration>(jsonContent, new JsonSerializerOptions()
            {
                // HelseID configuration is camelCase, overr
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                // Or: PropertyNameCaseInsensitive = true
            });
        }
    }
}
