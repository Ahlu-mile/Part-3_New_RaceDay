using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            var client = _factory.CreateClient();
            var dto = new RegisterDto
            {
                FullName = "Test Organiser",
                Email = $"organiser_{Guid.NewGuid()}@raceday.test",
                Password = "P@ssword123",
                Role = "Organiser"
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var client = _factory.CreateClient();
            var email = $"duplicate_{Guid.NewGuid()}@raceday.test";
            var dto = new RegisterDto
            {
                FullName = "First User",
                Email = email,
                Password = "P@ssword123",
                Role = "Participant"
            };

            await client.PostAsJsonAsync("/api/auth/register", dto);
            var secondResponse = await client.PostAsJsonAsync("/api/auth/register", dto);

            secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            var dto = new RegisterDto
            {
                FullName = "Bad Role User",
                Email = $"badrole_{Guid.NewGuid()}@raceday.test",
                Password = "P@ssword123",
                Role = "Administrator" // not a valid role
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkAndCreatesSession()
        {
            var client = _factory.CreateClient(); // HandleCookies defaults to true - session persists across calls
            var email = $"login_{Guid.NewGuid()}@raceday.test";
            var registerDto = new RegisterDto
            {
                FullName = "Login Test User",
                Email = email,
                Password = "P@ssword123",
                Role = "Participant"
            };
            await client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Email = email, Password = "P@ssword123" });

            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // The session cookie set on login should now let us reach a
            // protected endpoint without logging in again.
            var meResponse = await client.GetAsync("/api/users/me");
            meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var email = $"wrongpwd_{Guid.NewGuid()}@raceday.test";
            await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                FullName = "Wrong Password User",
                Email = email,
                Password = "CorrectPassword1",
                Role = "Participant"
            });

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Email = email, Password = "WrongPassword1" });

            loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithoutLogin_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
