namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IFeatureFlagService
{
    public bool IsFeatureEnabled(string featureName);
}
