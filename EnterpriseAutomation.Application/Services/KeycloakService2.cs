using EnterpriseAutomation.Application.Models;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoginRequest = EnterpriseAutomation.Application.Models.LoginRequest;
using RegisterRequest = EnterpriseAutomation.Application.Models.RegisterRequest;

namespace EnterpriseAutomation.Application.Services
{
    public class KeycloakService2 : IKeycloakService2
    {
        private readonly HttpClient _httpClient;
        private readonly KeycloakSettings _settings;

        public KeycloakService2(IOptions<KeycloakSettings> settings)
        {
            _settings = settings.Value;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"{_settings.AuthServerUrl}/realms/{_settings.Realm}/protocol/openid-connect/")
            };
        }

        public async Task<KeycloakResponse> LoginAsync(LoginRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret },
                { "grant_type", "password" },
                { "username", request.Username },
                { "password", request.Password }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync("token", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Keycloak login failed: {error}");
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<KeycloakResponse>(tokenResponse)
                ?? throw new Exception("Failed to deserialize token response");
        }

        public async Task<KeycloakResponse> RegisterAsync(RegisterRequest request)
        {
            // Get admin token first
            var adminToken = await GetAdminToken();

            // Create user in Keycloak
            var userJson = JsonSerializer.Serialize(new
            {
                username = request.Username,
                email = request.Email,
                enabled = true,
                firstName = request.FirstName,
                lastName = request.LastName,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = request.Password,
                        temporary = false
                    }
                }
            });

            var content = new StringContent(userJson, Encoding.UTF8, "application/json");
            var adminClient = new HttpClient();
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await adminClient.PostAsync(
                $"{_settings.AuthServerUrl}/admin/realms/{_settings.Realm}/users",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create user: {error}");
            }

            // Login the new user
            return await LoginAsync(new LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            });
        }

        private async Task<string> GetAdminToken()
        {
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret },
                { "grant_type", "client_credentials" }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync("token", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to get admin token");
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<KeycloakResponse>(tokenResponse);
            return token?.AccessToken ?? throw new Exception("Admin token is null");
        }
    }
}
