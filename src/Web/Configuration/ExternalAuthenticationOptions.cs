using System.Collections.Generic;
using System.Reflection;

namespace Diov.Web
{
    public class ExternalAuthenticationOptions
    {
        public GoogleAuthenticationOptions Google { get; set; }

        public ISchemeOptions GetSchemeOptions(string scheme)
        {
            return typeof(ExternalAuthenticationOptions)
                .GetProperty(
                    scheme,
                    BindingFlags.IgnoreCase |
                        BindingFlags.Instance |
                        BindingFlags.Public)?
                .GetValue(this) as ISchemeOptions;
        }

        public interface ISchemeOptions
        {
            IEnumerable<string> AdminAuthorization { get; set; }
        }

        public class GoogleAuthenticationOptions : ISchemeOptions
        {
            public IEnumerable<string> AdminAuthorization { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
    }
}