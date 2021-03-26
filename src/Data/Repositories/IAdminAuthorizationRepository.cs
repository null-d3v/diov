using System.Threading;
using System.Threading.Tasks;

namespace Diov.Data
{
    public interface IAdminAuthorizationRepository
    {
        Task<AdminAuthorization> GetAdminAuthorizationAsync(
            string accountId,
            string identityProvider,
            CancellationToken cancellationToken = default);
    }
}