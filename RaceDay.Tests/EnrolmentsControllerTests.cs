using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public EnrolmentsControllerTests(CustomWebApplicationFactory factory)
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

        private async Task<int> CreateEventWithCategoryAsync(HttpClient organiserClient)
        {
            var eventResponse = await organiserClient.PostAsJsonAsync("/api/events", new EventCreateDto
            {
                EventName = "Enrolment Test Event",
                EventType = "Run",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Gqeberha",
                StartTime = new TimeSpan(6, 0, 0)
            });
            var ev = await eventResponse.Content.ReadFromJsonAsync<EventResponseDto>();

            var categoryResponse = await organiserClient.PostAsJsonAsync(
                $"/api/events/{ev!.EventId}/categories", new CategoryCreateDto
                {
                    CategoryName = "10km",
                    DistanceKM = 10,
                    MaxParticipants = 2,
                    EntryFee = 100
                });
            var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponseDto>();

            return category!.CategoryId;
        }

        [Fact]
        public async Task Enrol_AsParticipant_ReturnsCreated()
        {
            var organiserClient = await RegisterAndLoginAsync("Organiser");
            var categoryId = await CreateEventWithCategoryAsync(organiserClient);

            var participantClient = await RegisterAndLoginAsync("Participant");
            var response = await participantClient.PostAsJsonAsync("/api/enrolments",
                new EnrolmentCreateDto { CategoryId = categoryId });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Enrol_AsOrganiser_ReturnsForbidden()
        {
            var organiserClient = await RegisterAndLoginAsync("Organiser");
            var categoryId = await CreateEventWithCategoryAsync(organiserClient);

            var anotherOrganiser = await RegisterAndLoginAsync("Organiser");
            var response = await anotherOrganiser.PostAsJsonAsync("/api/enrolments",
                new EnrolmentCreateDto { CategoryId = categoryId });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Enrol_Twice_ReturnsConflict()
        {
            var organiserClient = await RegisterAndLoginAsync("Organiser");
            var categoryId = await CreateEventWithCategoryAsync(organiserClient);

            var participantClient = await RegisterAndLoginAsync("Participant");
            await participantClient.PostAsJsonAsync("/api/enrolments",
                new EnrolmentCreateDto { CategoryId = categoryId });

            var secondAttempt = await participantClient.PostAsJsonAsync("/api/enrolments",
                new EnrolmentCreateDto { CategoryId = categoryId });

            secondAttempt.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task GetMyEnrolments_ReturnsOnlyOwnEnrolments()
        {
            var organiserClient = await RegisterAndLoginAsync("Organiser");
            var categoryId = await CreateEventWithCategoryAsync(organiserClient);

            var participantClient = await RegisterAndLoginAsync("Participant");
            await participantClient.PostAsJsonAsync("/api/enrolments",
                new EnrolmentCreateDto { CategoryId = categoryId });

            var response = await participantClient.GetAsync("/api/enrolments/me");
            var enrolments = await response.Content.ReadFromJsonAsync<List<EnrolmentResponseDto>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            enrolments.Should().HaveCount(1);
            enrolments![0].CategoryId.Should().Be(categoryId);
        }
    }
}
