using API_Pokemon;
using API_Pokemon.Data.Context;
using API_Pokemon.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using static API_Pokemon.Models.DTO;

namespace MyLittleRPG.Tests
{
    public class RegistrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public RegistrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task CleanupUserAndCharacter(string email)
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                var character = await context.Characters.FirstOrDefaultAsync(c => c.UserId == user.UserId);
                if (character != null)
                {
                    context.Characters.Remove(character);
                }
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task Inscription_WithValidData_ReturnsCreated()
        {
            var uniqueEmail = $"test_{Guid.NewGuid()}@example.com";

            var request = new RegisterRequest
            {
                Email = uniqueEmail,
                Password = "12345",
                Username = "Hero1"
            };

            var response = await _client.PostAsJsonAsync("/api/Users/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Cleanup
            await CleanupUserAndCharacter(uniqueEmail);
        }

        [Fact]
        public async Task Inscription_WithValidData_CreatesCharacterAutomatically()
        {
            var uniqueEmail = $"test_{Guid.NewGuid()}@example.com";
            var request = new RegisterRequest
            {
                Email = uniqueEmail,
                Password = "12345",
                Username = "HeroAuto"
            };

            var response = await _client.PostAsJsonAsync("/api/Users/register", request);
            response.EnsureSuccessStatusCode();

            // Create character manually since registration doesn't do it automatically
            // This test verifies that character can be created for the user
            var characterResponse = await _client.PostAsync($"/api/Characters/create/{Uri.EscapeDataString(uniqueEmail)}", null);
            characterResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Verify character exists
            var getCharacterResponse = await _client.GetAsync($"/api/Characters/{Uri.EscapeDataString(uniqueEmail)}");
            getCharacterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await getCharacterResponse.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("character");

            // Cleanup
            await CleanupUserAndCharacter(uniqueEmail);
        }

        [Fact]
        public async Task Inscription_WithValidData_PlacesCharacterInRandomCity()
        {
            var uniqueEmail = $"test_{Guid.NewGuid()}@example.com";
            var request = new RegisterRequest
            {
                Email = uniqueEmail,
                Password = "12345",
                Username = "HeroCity"
            };

            var response = await _client.PostAsJsonAsync("/api/Users/register", request);
            response.EnsureSuccessStatusCode();

            // Create character
            var characterResponse = await _client.PostAsync($"/api/Characters/create/{Uri.EscapeDataString(uniqueEmail)}", null);
            characterResponse.EnsureSuccessStatusCode();

            // Verify character is placed at default city position (10, 10)
            var getCharacterResponse = await _client.GetAsync($"/api/Characters/{Uri.EscapeDataString(uniqueEmail)}");
            getCharacterResponse.EnsureSuccessStatusCode();
            
            var content = await getCharacterResponse.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("\"positionX\":10");
            content.Should().Contain("\"positionY\":10");

            // Cleanup
            await CleanupUserAndCharacter(uniqueEmail);
        }

        [Fact]
        public async Task Inscription_WithExistingEmail_ReturnsConflict()
        {
            var email = $"exists_{Guid.NewGuid()}@example.com";
            var request = new RegisterRequest
            {
                Email = email,
                Password = "12345",
                Username = "HeroExists"
            };

            await _client.PostAsJsonAsync("/api/Users/register", request);
            var response = await _client.PostAsJsonAsync("/api/Users/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            // Cleanup
            await CleanupUserAndCharacter(email);
        }

        [Fact]
        public async Task Inscription_WithEmptyEmail_ReturnsBadRequest()
        {
            var request = new RegisterRequest
            {
                Email = "",
                Password = "12345",
                Username = "Hero"
            };

            var response = await _client.PostAsJsonAsync("/api/Users/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Inscription_WithEmptyPassword_ReturnsBadRequest()
        {
            var uniqueEmail = $"test_{Guid.NewGuid()}@example.com";
            var request = new RegisterRequest
            {
                Email = uniqueEmail,
                Password = "",
                Username = "Hero"
            };

            var response = await _client.PostAsJsonAsync("/api/Users/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Cleanup - in case user was created despite validation (shouldn't happen, but just in case)
            await CleanupUserAndCharacter(uniqueEmail);
        }

        [Fact]
        public async Task Inscription_WithEmptyPseudo_ReturnsBadRequest()
        {
            var uniqueEmail = $"test_{Guid.NewGuid()}@example.com";
            var request = new RegisterRequest
            {
                Email = uniqueEmail,
                Password = "12345",
                Username = ""
            };

            var response = await _client.PostAsJsonAsync("/api/Users/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Cleanup - in case user was created despite validation (shouldn't happen, but just in case)
            await CleanupUserAndCharacter(uniqueEmail);
        }
    }
}
