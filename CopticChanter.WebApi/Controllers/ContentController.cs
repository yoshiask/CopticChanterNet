using Microsoft.AspNetCore.Mvc;

namespace CopticChanter.WebApi.Controllers;

[Route("/content")]
public class ContentController : Controller
{
    [HttpGet]
    public IActionResult AvailableContent()
    {
        return Ok(WebApi.AvailableContent.List);
    }
}