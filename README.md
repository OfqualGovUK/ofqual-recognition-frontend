# Ofqual Register of Recognised Qualifications Frontend

[![Build Status](https://dev.azure.com/ofqual/Ofqual%20IM/_apis/build/status%2Fofqual-recognition-frontend?branchName=main)](https://dev.azure.com/ofqual/Ofqual%20IM/_build/latest?definitionId=393&branchName=main)

The Ofqual Register of Recognised Qualifications Frontend allows users to:

- Find out if a qualification can be recognised

## Provider

[The Office of Qualifications and Examinations Regulation](https://www.gov.uk/government/organisations/ofqual)

## About this project

This project is a ASP.NET Core 8 web app with the MVC architecture utilising Docker for deployment.

The web app runs on an App service for Container apps on Azure.

# Application Configuration Guide

This document outlines how the application is configured using `appsettings.json` files. These settings help manage behaviour across different environments and scenarios, including development, production and testing.

## Application Settings (`appsettings.json`)

The main application settings are defined in `appsettings.json` and can be tailored per environment using files like `appsettings.Development.json` or `appsettings.Production.json`.

```json
{
  "AzureAdB2C": {
    "Instance": "",
    "ClientId": "",
    "Domain": "",
    "SignUpSignInPolicyId": "",
    "SignUpSignInPolicyForAutomationId": "",
    "RedirectUri": "",
    "UseAutomationPolicies": false,
    "CallBackPath": "/signin-oidc",
    "AzureAdB2CSignedOutCallbackPath": "/signout-callback-oidc"
  },
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

### Setting Details

- **`AzureAdB2C:Instance`**
    The URL of the B2C service used to authenticate.

- **`AzureAdB2C:ClientId`**
    The Application ID of the service we will be running.

- **`AzureAdB2C:Domain`**
    The domain we will be authenticating under.

- **`AzureAdB2C:SignUpSignInPolicyId`**
    The policy name for the typical Sign up/Sign in flow.

- **`AzureAdB2C:SignUpSignInPolicyForAutomationId`**
    The policy name for the automated Sign up/Sign in flow, this should not be set in production environments.

- **`AzureAdB2C:RedirectUri`**
    An optional parameter that will override the sign-in redirect URL to the specified value, if specified. 
    This is used for the development service as a workaround and should not be required in production.
    
- **`AzureAdB2C:UseAutomationPolicies`**
    This flag is used in development to determine if the application uses the typical or automated Sign up/Sign in flow.
    This should only be set to `true` when using automated testing.

- **`AzureAdB2C:CallBackPath`**
    The callback path when signing in to Azure B2C, typically set to `/signin-oidc`

- **`AzureAdB2C:AzureAdB2CSignedOutCallbackPath`**
    The callback path when signing out of Azure B2c, typically set to `/signout-callback-oidc`
    
- **`RecognitionApi:BaseUrl`**  
  The base URL of the external Recognition API the application communicates with. This should point to the correct environment (e.g., local, development, production).

- **`LogzIo:Environment`**  
  Identifies the current environment in the logs (e.g., `DEV`, `PREPROD`, `PROD`). This helps separate log entries across environments.

- **`LogzIo:Uri`**  
  The endpoint URI for sending log data to an external logging service such as Logz.io.

- **`FeatureFlag:Application`**  
  A **boolean** flag used to enable or disable middleware URL redirection and application UI visibility.  
  When set to `false`, users will be redirected away from application-related routes and any associated buttons or links will not be shown.

> These settings should be environment-specific and managed through `appsettings.{Environment}.json` or overridden using environment variables in production scenarios.

## Test Settings (`appsettings.Test.json`)

These settings are used specifically for **Playwright-based end-to-end (E2E) and integration tests**. They define the runtime context for automated test execution and should not be applied in production.

```json
{
  "TestSettings": {
    "BaseUrl": ""
  }
}
```

### Setting Details

- **`TestSettings:BaseUrl`**  
  The base URL of the application under test. This should point to the environment where E2E or integration tests are executed (e.g., local dev server, test container, or staging instance).

> These settings support the test environment and can be overridden in CI/CD using environment variables (e.g., `TestSettings__BaseUrl`).
