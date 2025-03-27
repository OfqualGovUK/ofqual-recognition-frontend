using Microsoft.Extensions.Configuration;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _config;

    public FeatureFlagService(IConfiguration config)
    {
        _config = config;
    }

    public bool IsFeatureEnabled(string featureName)
    {
        var value = _config[$"FeatureFlag:{featureName}"];

        if (string.IsNullOrWhiteSpace(value))
            return false;
        
        return bool.TryParse(value, out var result) && result;
    }
}