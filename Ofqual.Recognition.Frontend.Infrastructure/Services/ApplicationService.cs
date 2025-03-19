using Microsoft.AspNetCore.Http;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ISession _session;

        public ApplicationService(IHttpContextAccessor httpContextAccessor) 
        {
            _session = httpContextAccessor!.HttpContext!.Session;
        }

        public async void CreateApplication()
        {
            try
            {
                var client = GetClient();
                var response = await GetFromJsonAsync<ApplicationModel>(baseUrlAndId);

                if (response != null)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            return new List<ApplicationModel>();
        }

        public ApplicationModel GetApplication()
        { 
            return new ApplicationModel
            {
                ApplicationId = _session.
                CreatedDate = _session.
                ModifiedDate = _session.
                CreatedByUpn = _session.
                ModifiedByUpn = _session.
            }
        }
    }
}
