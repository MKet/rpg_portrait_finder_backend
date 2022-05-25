using AuthenticationLibrary.Models;
using AuthenticationLibrary.Models.Output;
using AuthenticationService.Controllers;
using AuthenticationService.Data.Contexts;
using AuthenticationService.Data.Repositories.Implementations;
using AuthenticationService.Services;
using AuthenticationService.Services.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;

namespace AuthenticationServiceTest
{
    public class Tests
    {
        private UserRepository _repository;
        private EntityAuthenticationContext context;
        private IAuthenticationService _service;
        private readonly string privateKey = 
@"-----BEGIN RSA PRIVATE KEY-----
MIIBOwIBAAJBAMlAF1z0qxiDEHE2U75CbyTc4GQ1J7r8kl176iHRhe+ibHol2jMr
blW2LEz6ALAj2w5zn7GdCvvcwZABVSC6Y8sCAwEAAQJBALZmFkazohaXQ2G4gXHh
OGbKob1wx8+bdvSviGaaRbAHhFhNkZVru7L5MACCa99LTwyxvgGn8txwYj4S1noP
BjECIQDrwu39UpliZQVq4+z0Nqx396e3L350xj6mTVqvnUKnTQIhANqGv9UScGhD
l5BvENb9Xs97tsT4qwTD/Fx8C7G9v5t3AiAwu9OMGMXiA/XRuZmihaazCbteb2/Z
XZ1XrQfA42YCxQIgYiy1lEEzdPQg7IepVh0AclCPPRDGrF5sSxSDoHex1GMCIQCR
cKo09hVFvQ88oOc1b6ylJd5HZKsGLlj4lUrAcn3x5w==
-----END RSA PRIVATE KEY-----";
        private readonly string publicKey = 
@"-----BEGIN PUBLIC KEY-----
MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAMlAF1z0qxiDEHE2U75CbyTc4GQ1J7r8
kl176iHRhe+ibHol2jMrblW2LEz6ALAj2w5zn7GdCvvcwZABVSC6Y8sCAwEAAQ==
-----END PUBLIC KEY-----";
        private readonly string testUsername = "bob322";
        private readonly string testPassword = "password";

        [SetUp]
        public async Task Setup()
        {
            var options =  new DbContextOptionsBuilder<EntityAuthenticationContext>().UseInMemoryDatabase(databaseName: "InMemoryAuthDb").Options;
            context = new EntityAuthenticationContext(options);
            await context.SeedAsync();
            _repository = new UserRepository(context);
            _service = new JwtAuthenticationService(new UserRepository(context), privateKey, publicKey, null);
        }

        [Test]
        public async Task AuthenticateSuccessTokenSet()
        {
            var token = await _service.LoginAsync(testUsername, testPassword);
            
            Assert.That(token, Is.Not.Null);
        }

        [Test]
        public async Task AuthenticateSuccessTokenVerifiable()
        {
            var token = await _service.LoginAsync(testUsername, testPassword);

            var verified = await _service.Verify(token.Token);

            Assert.That(verified, Is.True);
        }

        [Test]
        public async Task AuthenticateWrongUsername()
        {
            var token = await _service.LoginAsync("UNKNOWN", testPassword);

            Assert.That(token, Is.Null);
        }

        [Test]
        public async Task AuthenticateWrongUsernameControllerResponse()
        {
            AuthenticationController controller = new AuthenticationController(_service);
            var response = await controller.Authenticate(new AuthenticationData("UNKNOWN", testPassword));

            Assert.That(response.Result, Is.TypeOf<UnauthorizedResult>() | Is.TypeOf<UnauthorizedObjectResult>());
        }


        [Test]
        public async Task AuthenticateWrongPassword()
        {
            var token = await _service.LoginAsync(testUsername, "wrong");

            Assert.That(token, Is.Null);
        }

        [Test]
        public async Task AuthenticateWrongPasswordControllerResponse()
        {
            AuthenticationController controller = new AuthenticationController(_service);
            var response = await controller.Authenticate(new AuthenticationData(testUsername, "wrong"));

            Assert.That(response.Result, Is.TypeOf<UnauthorizedResult>() | Is.TypeOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task AuthenticateDifferingToken()
        {
            var localPublicKey =
                @"-----BEGIN PUBLIC KEY-----
MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBANoXTolhTJjvAJAiDKr7PAJ+ZcE3KOXa
cGLZIZMsysoxYiVcPNicm2lhSi3VC/rkn5RFlsQnjG2P7F+FZUcx/CUCAwEAAQ==
-----END PUBLIC KEY-----";
            var token = await _service.LoginAsync(testUsername, testPassword);
            var otherTokenService = new JwtAuthenticationService(new UserRepository(context), privateKey, localPublicKey, null);

            var verified = await otherTokenService.Verify(token.Token);

            Assert.That(verified, Is.False);
        }

        [TearDown]
        public async Task TearDown()
        {
            await context.DisposeAsync();
        }
    }
}