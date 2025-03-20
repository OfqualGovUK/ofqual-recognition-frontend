using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces
{
    public interface IApplicationService
    {
        Task<Application?> SetUpApplication();
    }
}
