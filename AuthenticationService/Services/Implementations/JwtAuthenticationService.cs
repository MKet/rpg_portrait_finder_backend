using AuthenticationLibrary.Models.Output;
using AuthenticationService.Data;
using AuthenticationService.Data.Repositories;
using Azure.Security.KeyVault.Keys;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationService.Services.Implementations
{
    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtAlgorithm _jwtAlgorithm;
        private readonly ILogger<JwtAuthenticationService>? _logger;

        public JwtAuthenticationService(IUserRepository userRepository, IJwtAlgorithm jwtAlgorithm, ILogger<JwtAuthenticationService>? logger)
        {
            _userRepository = userRepository;
            _jwtAlgorithm = jwtAlgorithm;
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

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            string hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password);
            bool userAdded = await _userRepository.AddUser(username, email, hash);

            return userAdded;
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
                .WithAlgorithm(_jwtAlgorithm);

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
