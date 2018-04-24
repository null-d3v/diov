using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Diov.Web
{
    public class AuthController : Controller
    {
        [HttpGet("/callback")]
        public async Task<IActionResult> Callback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(
                Constants.ExternalAuthenticationScheme);

            // TODO Authorization whitelist.

            await HttpContext.SignOutAsync(
                Constants.ExternalAuthenticationScheme);

            await HttpContext.SignInAsync(
                Constants.LocalAuthenticationScheme,
                authenticateResult.Principal);

            var returnUrl = authenticateResult
                .Properties
                .Items["returnUrl"];
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToRoute("Admin");
        }

        [HttpGet("/challenge/{scheme}")]
        public IActionResult Challenge(
            string scheme,
            string returnUrl = null)
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", scheme },
                }
            };

            return Challenge(authenticationProperties, scheme);
        }

        [HttpGet("/login")]
        public IActionResult LogIn(string returnUrl)
        {
            return View(
                new LogInModel
                {
                    ReturnUrl = returnUrl,
                });
        }
    }
}