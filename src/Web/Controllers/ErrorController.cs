using Microsoft.AspNetCore.Mvc;

namespace Diov.Web;

[Route("[controller]")]
public class ErrorController : Controller
{
    [HttpGet("{error}")]
    public IActionResult Detail(
        [FromRoute] string error)
    {
        return View(error);
    }
}
