using System.Diagnostics.CodeAnalysis;

namespace Diov.Web;

public class LoginModel
{
    public ExternalAuthenticationOptions
        ExternalAuthenticationOptions { get; set; } = default!;

    [SuppressMessage("Design", "CA1056")]
    public string ReturnUrl { get; set; } = default!;
}
