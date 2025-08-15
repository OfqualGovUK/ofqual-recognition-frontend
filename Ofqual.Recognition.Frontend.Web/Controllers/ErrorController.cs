using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    private readonly HelpDeskContact _helpDeskContact;

    public ErrorController(HelpDeskContact helpDeskContact)
    {
        _helpDeskContact = helpDeskContact;
    }

    [Route("404")]
    public IActionResult NotFoundError()
    {
        var viewModel = ErrorMapper.MapToViewModel(_helpDeskContact);

        return View("NotFound", viewModel);
    }

    [Route("500")]
    public IActionResult InteralServerError()
    {
        var viewModel = ErrorMapper.MapToViewModel(_helpDeskContact);

        return View("Problem", viewModel);
    }

    [Route("400")]
    public IActionResult BadRequestError()
    {
        var viewModel = ErrorMapper.MapToViewModel(_helpDeskContact);

        return View("Problem", viewModel);
    }
}
