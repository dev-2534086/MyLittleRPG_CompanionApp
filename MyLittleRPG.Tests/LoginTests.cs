using API_Pokemon;
using API_Pokemon.Data.Context;
using API_Pokemon.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using static API_Pokemon.Models.DTO;

namespace MyLittleRPG.Tests
{
    public class LoginTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public LoginTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task SeedUser(string email, string password, string username)
        {
            var request = new RegisterRequest
            {
                Email = email,
                Password = password,
                Username = username
            };
            await _client.PostAsJsonAsync("/api/Users/register", request);
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
        public async Task Connexion_WithValidCredentials_ReturnsOk()
        {
            var uniqueEmail = $"login_{Guid.NewGuid()}@example.com";
            await SeedUser(uniqueEmail, "12345", "HeroLogin");

            var request = new LoginRequest { Email = uniqueEmail, Password = "12345" };
            var response = await _client.PostAsJsonAsync("/api/Users/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Cleanup
            await CleanupUserAndCharacter(uniqueEmail);
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_SetsEstConnecteToTrue()
        {
            var uniqueEmail = $"login_{Guid.NewGuid()}@example.com";
            await SeedUser(uniqueEmail, "12345", "HeroLogin2");

            var request = new LoginRequest { Email = uniqueEmail, Password = "12345" };
            var response = await _client.PostAsJsonAsync("/api/Users/login", request);

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonContent);
            var userElement = jsonDoc.RootElement.GetProperty("user");
            var isConnected = userElement.GetProperty("isConnected").GetBoolean();

            isConnected.Should().BeTrue();

            // Cleanup
            await CleanupUserAndCharacter(uniqueEmail);
        }

        [Fact]
        public async Task Connexion_WithValidCredentials_AllowsSubsequentAuthenticatedRequests()
        {
            var uniqueEmail = $"login_{Guid.NewGuid()}@example.com";
            await SeedUser(uniqueEmail, "12345", "HeroAuth");

            var loginRequest = new LoginRequest { Email = uniqueEmail, Password = "12345" };
            var loginResponse = await _client.PostAsJsonAsync("/api/Users/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            // Verify user is connected by checking IsConnected flag
            var jsonContent = await loginResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonContent);
            var userElement = jsonDoc.RootElement.GetProperty("user");
            var isConnected = userElement.GetProperty("isConnected").GetBoolean();
            isConnected.Should().BeTrue();

            // Create character and verify we can access it (simulating authenticated request)
            var characterResponse = await _client.PostAsync($"/api/Characters/create/{Uri.EscapeDataString(uniqueEmail)}", null);
            characterResponse.EnsureSuccessStatusCode();

            var getCharacterResponse = await _client.GetAsync($"/api/Characters/{Uri.EscapeDataString(uniqueEmail)}");
            getCharacterResponse.EnsureSuccessStatusCode();

            // Cleanup
            await CleanupUserAndCharacter(uniqueEmail);
        }

        [Fact]
        public async Task Connexion_WithInvalidEmail_ReturnsUnauthorized()
        {
            var request = new LoginRequest { Email = $"nonexistent_{Guid.NewGuid()}@example.com", Password = "12345" };
            var response = await _client.PostAsJsonAsync("/api/Users/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Connexion_WithInvalidPassword_ReturnsUnauthorized()
        {
            var uniqueEmail = $"login_{Guid.NewGuid()}@example.com";
            await SeedUser(uniqueEmail, "12345", "HeroLogin");

            var request = new LoginRequest { Email = uniqueEmail, Password = "wrongpassword" };
            var response = await _client.PostAsJsonAsync("/api/Users/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Cleanup
            await CleanupUserAndCharacter(uniqueEmail);
        }

        [Fact]
        public async Task Connexion_WithNonexistentUser_ReturnsUnauthorized()
        {
            var request = new LoginRequest { Email = $"nonexistent_{Guid.NewGuid()}@example.com", Password = "12345" };
            var response = await _client.PostAsJsonAsync("/api/Users/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Connexion_WithEmptyCredentials_ReturnsBadRequest()
        {
            var request = new LoginRequest { Email = "", Password = "" };
            var response = await _client.PostAsJsonAsync("/api/Users/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
