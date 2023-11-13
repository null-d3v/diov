namespace Diov.Web;

public class GoogleAuthenticationOptions : ISchemeOptions
{
    public string? AdminAuthorization { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }
}