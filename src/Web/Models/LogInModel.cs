namespace Diov.Web;

public class LoginModel
{
    public ExternalAuthenticationOptions
        ExternalAuthenticationOptions { get; set; } = default!;

    public string ReturnUrl { get; set; } = default!;
}
