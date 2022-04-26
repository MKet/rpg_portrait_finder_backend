using AuthenticationLibrary.Models.Output;

namespace AuthenticationService.Services
{
    public interface IAuthenticationService
    {
        Task<AuthToken?> LoginAsync(string username, string pasword);
    }
}
