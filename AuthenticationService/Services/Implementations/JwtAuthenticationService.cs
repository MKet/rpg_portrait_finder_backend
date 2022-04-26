using AuthenticationLibrary.Models.Output;
using AuthenticationService.Data.Repositories;
using JWT.Algorithms;
using JWT.Builder;

namespace AuthenticationService.Services.Implementations
{
    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _secret;

        public JwtAuthenticationService(IUserRepository userRepository, string secret) 
        {
            _userRepository = userRepository;
            _secret = secret;
        }

        public async Task<AuthToken?> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user is not null && BCrypt.Net.BCrypt.Verify(password, user.Password, true))
            {
                var token = JwtBuilder.Create()
                          .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                          .WithSecret(_secret)
                          .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                          .Encode();
                return new AuthToken(token);
            }
            else
            {
                return null;
            }
        }
    }
}
