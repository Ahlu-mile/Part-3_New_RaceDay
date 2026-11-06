using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public EventsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> RegisterAndLoginAsync(string role)
        {
            var client = _factory.CreateClient();
            var email = $"{role.ToLower()}_{Guid.NewGuid()}@raceday.test";

            await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                FullName = $"Test {role}",
                Email = email,
                Password = "P@ssword123",
                Role = role
            });

            await client.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Email = email, Password = "P@ssword123" });

            return client;
        }

        [Fact]
        public async Task GetEvents_WithoutLogin_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/events");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            var client = await RegisterAndLoginAsync("Organiser");

            var response = await client.PostAsJsonAsync("/api/events", new EventCreateDto
            {
                EventName = "Test 10km Run",
                EventType = "Run",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Gqeberha",
                StartTime = new TimeSpan(6, 0, 0)
            });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            var client = await RegisterAndLoginAsync("Participant");

            var response = await client.PostAsJsonAsync("/api/events", new EventCreateDto
            {
                EventName = "Should Not Be Created",
                EventType = "Walk",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Gqeberha",
                StartTime = new TimeSpan(7, 0, 0)
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/events", new EventCreateDto
            {
                EventName = "Anonymous Event",
                EventType = "Cycle",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Gqeberha",
                StartTime = new TimeSpan(8, 0, 0)
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteEvent_AsNonOwnerOrganiser_ReturnsForbidden()
        {
            var ownerClient = await RegisterAndLoginAsync("Organiser");
            var createResponse = await ownerClient.PostAsJsonAsync("/api/events", new EventCreateDto
            {
                EventName = "Owner's Event",
                EventType = "Run",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Gqeberha",
                StartTime = new TimeSpan(6, 0, 0)
            });
            var created = await createResponse.Content.ReadFromJsonAsync<EventResponseDto>();

            var otherOrganiserClient = await RegisterAndLoginAsync("Organiser");
            var deleteResponse = await otherOrganiserClient.DeleteAsync($"/api/events/{created!.EventId}");

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
