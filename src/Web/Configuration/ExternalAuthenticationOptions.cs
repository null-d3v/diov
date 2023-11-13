using System.Reflection;

namespace Diov.Web;

public class ExternalAuthenticationOptions
{
    public GoogleAuthenticationOptions? Google { get; set; }

    public ISchemeOptions? GetSchemeOptions(string scheme)
    {
        return typeof(ExternalAuthenticationOptions)
            .GetProperty(
                scheme,
                BindingFlags.IgnoreCase |
                    BindingFlags.Instance |
                    BindingFlags.Public)?
            .GetValue(this) as ISchemeOptions;
    }
}
