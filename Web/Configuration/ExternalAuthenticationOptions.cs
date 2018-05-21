namespace Diov.Web
{
    public class ExternalAuthenticationOptions
    {
        public GoogleAuthenticationOptions Google { get; set; }

        public class GoogleAuthenticationOptions
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
    }
}