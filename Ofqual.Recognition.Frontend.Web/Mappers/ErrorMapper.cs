using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public static class ErrorMapper
{
    public static ErrorViewModel MapToViewModel(HelpDeskContact helpDeskContact)
    {
        return new ErrorViewModel
        {
            ContactName = helpDeskContact.ContactName,
            Url = helpDeskContact.Url
        };
    }
}