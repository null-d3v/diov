namespace Diov.Web
{
    public class LoginModel
    {
        public ExternalAuthenticationOptions ExternalAuthenticationOptions { get; set; }
        public string ReturnUrl { get; set; }
    }
}