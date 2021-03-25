namespace Diov.Data
{
    public class AdminAuthorization
    {
        private string identityProvider;

        public string AccountId { get; set; }
        public int Id { get; set; }
        public string IdentityProvider
        {
            get
            {
                return identityProvider;
            }
            set
            {
                identityProvider = value.ToLowerInvariant();
            }
        }
    }
}