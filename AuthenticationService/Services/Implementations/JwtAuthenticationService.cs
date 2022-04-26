using AuthenticationLibrary.Models.Output;
using AuthenticationService.Data;
using AuthenticationService.Data.Repositories;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;

namespace AuthenticationService.Services.Implementations
{
    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _secret;
        private readonly ILogger<JwtAuthenticationService>? _logger;

        public JwtAuthenticationService(IUserRepository userRepository, string secret, ILogger<JwtAuthenticationService>? logger)
        {
            _userRepository = userRepository;
            _secret = secret;
            _logger = logger;
        }

        public async Task<AuthToken?> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user is not null && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
            {
                var token = EncodeJwt(user);
                return new AuthToken(token);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> Verify(string token)
        {
            try
            {
                var JwtValues = CreateJwtBuilder()
                    .MustVerifySignature()
                    .Decode<IDictionary<string, object>>(token);

                var parseSuccess = int.TryParse(JwtValues["user"].ToString(), out int userId);

                return parseSuccess && await _userRepository.GetUserAsync(userId) is not null;
            } 
            catch (SignatureVerificationException e)
            {
                _logger?.LogWarning("JWT Signature incorrect: {Expected} expected, {Received} received", e.Expected, e.Received);
                return false;
            }
        }

        private JwtBuilder CreateJwtBuilder()
            => JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                .WithSecret(_secret);

        private string EncodeJwt(User user)
        {
            return CreateJwtBuilder()
                    .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                    .AddClaim("username", user.Username)
                    .AddClaim("user", user.Id)
                    .Encode();

        } 
    }
}
