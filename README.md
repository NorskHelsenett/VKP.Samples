# VKP Samples
Velferdsteknologisk Knutepunkt (VKP) offers data sharing in the healthcare sector. 
The service standardizes, translates, and transports data between different systems that 
initially do not interact. For instance, VKP can ensure that data from a welfare technology 
system is automatically recorded in the municipality's patient record system, so the 
municipal healthcare staff do not have to manually enter this information.

In this way, VKP supports the development of new healthcare services in the use of welfare 
technology and digital home monitoring. The main purpose of the service is to contribute 
to the development of a sustainable healthcare system, using technological solutions 
that provide more efficient resource use and better opportunities for individual 
customization of services for each citizen.

Read more about VKP on the Norsk Helsenett's website:
https://www.nhn.no/tjenester/velferdsteknologisk-knutepunkt/hva-er-tilgjengelig-i-vkp

## Prerequisite knowledge
VKP API-s are protected using HelseID which is a national authentication service for the 
healthcare sector in Norway. HelseID is based on [OAuth 2.0 Framework](https://oauth.net/2/) See https://www.nhn.no/tjenester/helseid for more information.

## Basic samples
This repository contains a Visual Studio solution with basic samples demonstrating how to use VKP API endpoints. The endpoints are documented in the developer's documentation pages, see https://utviklerportal.nhn.no/informasjonstjenester/velferdsteknologisk-knutepunkt-vkp.

The following sample projects are included:

* `AllergyIntoleranceSearch`
* `CareplanSearch`
* `PatientSearch`
