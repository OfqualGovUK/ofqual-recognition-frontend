# Ofqual Register of Recognised Qualifications Frontend

[![Build Status](https://dev.azure.com/ofqual/Ofqual%20IM/_apis/build/status%2Fofqual-recognition-frontend?branchName=main)](https://dev.azure.com/ofqual/Ofqual%20IM/_build/latest?definitionId=393&branchName=main)

The Ofqual Register of Recognised Qualifications Frontend allows users to:

- Find out if a qualification can be recognised

## Provider

[The Office of Qualifications and Examinations Regulation](https://www.gov.uk/government/organisations/ofqual)

## About this project

This project is a ASP.NET Core 8 web app with the MVC architecture utilising Docker for deployment.

The web app runs on an App service for Container apps on Azure.

### App Settings Definition

The following configuration structure is used in `appsettings.json`. Each section defines settings that control application behaviour across different environments.

```JSON
{
 "RecognitionApi": {
   "BaseUrl": ""
 },
 "LogzIo": {
   "Environment": "",
   "Uri": ""
 },
 "FeatureFlag": {
   "Application": ""
 }
}
```

#### Setting Descriptions

- `RecognitionApi:BaseUrl`

  The base URL of the external Recognition API the application communicates with. This should point to the appropriate environment (e.g., local, development, production).

- `LogzIo:Environment`

  A label for identifying the current environment (e.g., DEV, PREPROD, PROD) in logs.

- `LogzIo:Uri`

  The endpoint URI for sending logs to Logz.io or another external logging service.

- `FeatureFlag:Application`

   Used to enable or disable both the middleware URL redirection and the visibility of the application-related UI. When disabled, users will be redirected away from application routes and the button or link to access them will not be rendered.


> These settings should be environment-specific and managed through `appsettings.{Environment}.json` or environment variables in production scenarios.

### Test Settings Definition

The following configuration structure is used in `appsettings.Test.json`. These settings are specifically for automated testing scenarios and help define the test environment context.

```JSON
{
 "TestSettings": {
   "BaseUrl": ""
 }
}
```

#### Setting Descriptions

- `TestSettings:BaseUrl`  
  The base URL of the application under test. This should point to the environment where tests are executed (e.g., a locally hosted app, test server, or staging environment).

> This setting can be overridden via environment variables in CI/CD pipelines using `TestSettings__YourCustomVariable`.
