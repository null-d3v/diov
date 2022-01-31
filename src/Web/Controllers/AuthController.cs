using Diov.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Diov.Web;

[Route("[controller]")]
public class AuthController : Controller
{
    public AuthController(
        IAdminAuthorizationRepository adminAuthorizationRepository,
        IOptions<ExternalAuthenticationOptions> externalAuthenticationOptions)
    {
        AdminAuthorizationRepository = adminAuthorizationRepository;
        ExternalAuthenticationOptions = externalAuthenticationOptions.Value;
    }

    public IAdminAuthorizationRepository AdminAuthorizationRepository { get; }
    public ExternalAuthenticationOptions ExternalAuthenticationOptions { get; }

    [HttpGet("[action]")]
    public async Task<IActionResult> Callback(
        CancellationToken cancellationToken = default)
    {
        var authenticateResult = await HttpContext
            .AuthenticateAsync(
                Constants.ExternalAuthenticationScheme);
        if (!authenticateResult.Succeeded ||
            authenticateResult.Principal == null)
        {
            return Unauthorized();
        }

        var nameIdentifier = authenticateResult.Principal?
            .Claims.FirstOrDefault(
                claim => claim.Type == ClaimTypes.NameIdentifier)?
            .Value;
        var scheme = authenticateResult.Ticket?
            .Properties.Items["scheme"];
        if (nameIdentifier == null || scheme == null)
        {
            return Unauthorized();
        }

        if (ExternalAuthenticationOptions
            .GetSchemeOptions(scheme)?
            .AdminAuthorization != nameIdentifier)
        {
            var adminAuthorization = await AdminAuthorizationRepository
                .GetAdminAuthorizationAsync(
                    nameIdentifier, scheme, cancellationToken);
            if (adminAuthorization == null)
            {
                return Unauthorized();
            }
        }

        await HttpContext.SignOutAsync(
            Constants.ExternalAuthenticationScheme);

        await HttpContext.SignInAsync(
            Constants.LocalAuthenticationScheme,
            authenticateResult.Principal!); // HACK Unnecessary

        var returnUrl = authenticateResult
            .Properties
            .Items["returnUrl"];
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Content");
    }

    [HttpGet("[action]/{scheme}")]
    public IActionResult Challenge(
        [FromRoute]string scheme,
        [FromQuery]string? returnUrl = null)
    {
        var authenticationProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            Items =
            {
                { "returnUrl", returnUrl },
                { "scheme", scheme },
            },
            RedirectUri = Url.Action(nameof(Callback)),
        };

        return Challenge(authenticationProperties, scheme);
    }

    [HttpGet("[action]")]
    public IActionResult Login(
        [FromQuery]string returnUrl)
    {
        return View(
            new LoginModel
            {
                ExternalAuthenticationOptions =
                    ExternalAuthenticationOptions,
                ReturnUrl = returnUrl,
            });
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return RedirectToAction("Index", "Content");
    }
}