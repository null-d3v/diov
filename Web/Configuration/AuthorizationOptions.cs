using System.Collections.Generic;

namespace Diov.Web
{
    public class AuthorizationOptions
    {
        public GoogleAuthorizationOptions Google { get; set; }

        public class GoogleAuthorizationOptions
        {
            public IEnumerable<long> AccountIds { get; set; }
        }
    }
}