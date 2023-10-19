##### Disclaimer
*This sample is a work in progress and might become updated without notice.*

# PatientSearch sample
This sample shows how to perform a patient search using the VKP API. 

## Dependencies
### IdentityModel
We use IdentityModel to simplify the use of OAuth2 in our application.
### Firely
Firely is used for serialization and deserialization of FHIR-resources returned from the API.
### OneOf
OneOf is used to provide F# style unions in C# as the API can return many types, 
e.g. a Bundle and an OperationOutcome. 

## HelseID configuration
Create a new HelseID configuration at https://selvbetjening.test.nhn.no/. `VKP-TestVFT` can be used as the client system. 
Select the neccessary scopes, for this example it is sufficient with patient reading access (nhn:vkp/api/user/patient.read).

Download the configuration and save it locally as `HelseID.json`. 
See `HelseID-sample.json` for an example of how this file looks.
For production scenarios the key materials must be sufficiently secured. 
For this test the configuration is included as a json-file, 
in a production environment the private key must be sufficiently secured.

## Running the sample
 The sample has been pre-configured to use organization number *888134576* patient id *13116900216* (Line Danser).
HelseID.json file has not been included and must be saved locally after completed HelseID configuration.
