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
        return _config.GetValue($"FeatureFlag:{featureName}", false);
    }
}