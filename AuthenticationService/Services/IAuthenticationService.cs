using AuthenticationLibrary.Models.Output;

namespace AuthenticationService.Services
{
    public interface IAuthenticationService
    {
        Task<AuthToken?> LoginAsync(string username, string pasword);

        Task<bool> Verify(string token);
        Task<bool> RegisterAsync(string username, string Email, string password);
    }
}
