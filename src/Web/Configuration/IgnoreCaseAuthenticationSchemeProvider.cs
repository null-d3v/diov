using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Diov.Web;

public class IgnoreCaseAuthenticationSchemeProvider(
    IOptions<AuthenticationOptions> optionsAccessor) :
    AuthenticationSchemeProvider(
        optionsAccessor,
        new Dictionary<string, AuthenticationScheme>(
            StringComparer.OrdinalIgnoreCase))
{
}
