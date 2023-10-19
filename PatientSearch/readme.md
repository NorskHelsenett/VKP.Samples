# PatientSearch sample
This sample shows how to perform a patient search using the VKP API. 

## Dependencies
### IdentityModel
We use IdentityModel to simplify the use of OAuth2.
### Firely


## HelseID configuration
Create a new HelseID configuration at https://selvbetjening.test.nhn.no/. `VKP-TestVFT` can be used as the client system. 
Select the neccessary scopes, for this example it is sufficient with patient reading access (nhn:vkp/api/user/patient.read).

Download the configuration and save it locally. 
For production scenarios the key materials must be sufficiently secured. 
For this test the configuration is included as a json-file.

