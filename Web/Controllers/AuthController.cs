using Diov.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Diov.Web
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        public AuthController(
            IAdminAuthorizationRepository adminAuthorizationRepository)
        {
            AdminAuthorizationRepository = adminAuthorizationRepository ??
                throw new ArgumentNullException(nameof(adminAuthorizationRepository));
        }

        public IAdminAuthorizationRepository AdminAuthorizationRepository { get; }

        [HttpGet("[action]")]
        public async Task<IActionResult> Callback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(
                Constants.ExternalAuthenticationScheme);

            var nameIdentifier = authenticateResult.Principal.Claims.FirstOrDefault(
                claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            var adminAuthorization = await AdminAuthorizationRepository
                .GetAdminAuthorizationAsync(
                    nameIdentifier,
                    authenticateResult.Ticket.Properties.Items["scheme"]);
            if (adminAuthorization == null)
            {
                return Unauthorized();
            }

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

        [HttpGet("[action]/{scheme}")]
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

        [HttpGet("[action]")]
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