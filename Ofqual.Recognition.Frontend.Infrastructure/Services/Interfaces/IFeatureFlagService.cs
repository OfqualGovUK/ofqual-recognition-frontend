namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IFeatureFlagService
{
    bool IsFeatureEnabled(string featureName);
}
