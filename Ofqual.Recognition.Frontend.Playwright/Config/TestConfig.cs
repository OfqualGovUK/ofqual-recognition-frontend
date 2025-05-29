using Microsoft.Extensions.Configuration;

namespace Ofqual.Recognition.Frontend.Playwright.Configs;

public static class TestConfig
{
    private static readonly IConfigurationRoot _config;

    static TestConfig()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Test.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static string RecognitionBaseUrl => _config["TestSettings:BaseUrl"]!;
    public static string B2CUsername => _config["TestSettings:B2CUser:Username"]!;
    public static string B2CPassword => _config["TestSettings:B2CUser:Password"]!;
}