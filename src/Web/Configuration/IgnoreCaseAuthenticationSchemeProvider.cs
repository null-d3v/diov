using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Diov.Web
{
    public class IgnoreCaseAuthenticationSchemeProvider :
        AuthenticationSchemeProvider
    {
        public IgnoreCaseAuthenticationSchemeProvider(
            IOptions<AuthenticationOptions> options) :
            base(
                options,
                new Dictionary<string, AuthenticationScheme>(
                    StringComparer.OrdinalIgnoreCase))
        {
        }
    }
}