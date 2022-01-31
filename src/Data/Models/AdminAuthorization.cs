using System.ComponentModel.DataAnnotations;

namespace Diov.Data;

public class AdminAuthorization
{
    private string identityProvider = default!;

    [Required]
    public string AccountId { get; set; } = default!;

    public int Id { get; set; }

    [Required]
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
