using IdentityModel;
using IdentityModel.Client;
using PatientSearch;


var helseIdConfiguration = HelseIdConfiguration.ReadFromFile("HelseID.json");

var httpClient = new HttpClient();
var client = new VkpClient(helseIdConfiguration, httpClient);

var response = await client.PatientSearchAsync("888134576");

//var stsAddress = "https://helseid-sts.test.nhn.no/";
//var clientId = "277852af-202b-4f36-983f-2b125b7928d6";
//var clientSecret = "";

//var orgNo = "888134576";

//// Fetch the discovery document
//var client = new HttpClient();

//var disco = await client.GetDiscoveryDocumentAsync(stsAddress);
//if (disco.IsError) throw new Exception(disco.Error);

//// Get a token
//var response = await client.RequestTokenAsync(new TokenRequest()
//{
//    Address = disco.TokenEndpoint,
//    GrantType = OidcConstants.GrantTypes.ClientCredentials,
//    ClientId = clientId,
//    ClientSecret = clientSecret
//});

//if (response.IsError) throw new Exception(response.Error);

//var httpClient = new HttpClient()
//{
//    BaseAddress = new Uri(apiAddress)
//};

//var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{orgNo}/patient/_search");

//var response = await httpClient.SendAsync(httpRequest);

//response.EnsureSuccessStatusCode();