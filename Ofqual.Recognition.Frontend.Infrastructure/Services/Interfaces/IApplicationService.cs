using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces
{
    public interface IApplicationService
    {
        void CreateApplication();
        ApplicationModel GetApplication();
    }
}
