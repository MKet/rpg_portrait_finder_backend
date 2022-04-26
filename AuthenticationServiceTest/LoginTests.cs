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
        private readonly string _secret = "MY_SECRET";
        private readonly string testUsername = "bob322";
        private readonly string testPassword = "password";

        [SetUp]
        public async Task Setup()
        {
            var options =  new DbContextOptionsBuilder<EntityAuthenticationContext>().UseInMemoryDatabase(databaseName: "InMemoryAuthDb").Options;
            context = new EntityAuthenticationContext(options);
            await context.SeedAsync();
            _repository = new UserRepository(context);
            _service = new JwtAuthenticationService(new UserRepository(context), _secret, null);
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
            var token = await _service.LoginAsync(testUsername, testPassword);
            var otherTokenService = new JwtAuthenticationService(new UserRepository(context), "DIFFERENT_SECRET", null);

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