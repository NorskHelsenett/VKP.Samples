##### Disclaimer
*This sample is a work in progress and might become updated without notice.*

## Dependencies
### IdentityModel
We use IdentityModel to simplify the use of OAuth2 in our application.
### Firely
Firely is used for serialization and deserialization of FHIR-resources returned from the API.
### OneOf
OneOf is used to provide F# style unions in C# as the API can return many types, 
e.g. a Bundle and an OperationOutcome. 

## HelseID configuration
*Disclaimer: For production scenarios the key materials must be sufficiently secured, 
and should not be included in source control.*

This sample contains an existing configuration created at https://selvbetjening.test.nhn.no/.
This configuration has also been approved by VKP.

If you create your own configuration it will have to be approved by VKP.

For this test the configuration is included as a json-file, 
in a production environment the private key must be sufficiently secured.
