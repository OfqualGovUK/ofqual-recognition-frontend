Playwright installation:

1. Install Playwright for .NET 
Run the following command in your project's root directory:

    dotnet add package Microsoft.Playwright

2. Install Playwright Browsers 
After adding Playwright to your project, install the required browsers:

    pwsh bin/Debug/netX/playwright.ps1 install

--------------------------------------------------------------------------------
Running Tests locally:

1. Add appsettings.Test.development.json file with following details:

{
    "TestSettings": {
        "BaseUrl": "https://localhost:7072",
        "B2CUser": {
            "Username": "username",
            "Password": "password"
        }
    }
}

2. Start the frontend app in https mode

2. To run the tests in headless mode:

    dotnet test

3. To run the tests in headed mode:

    dotnet test --settings:WithHead.runsettings