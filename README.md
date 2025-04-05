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

> This setting can be overridden in CI/CD pipelines using environment variables in the format: `TestSettings__BaseUrl`.
