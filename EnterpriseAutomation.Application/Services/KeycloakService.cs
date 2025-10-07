using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models;
using EnterpriseAutomation.Application.Models.Users;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Application.Utilities;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LoginRequest = EnterpriseAutomation.Application.Models.LoginRequest;
using RegisterRequest = EnterpriseAutomation.Application.Models.RegisterRequest;

namespace EnterpriseAutomation.Application.Services
{   

    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly KeycloakSettings _settings;
        private readonly IRepository<User> _userRepository;

        public KeycloakService(IHttpClientFactory httpClientFactory, 
            IOptions<KeycloakSettings> settings,IRepository<User> userRepository)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri($"{_settings.AuthServerUrl}/realms/{_settings.Realm}/protocol/openid-connect/");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _userRepository = userRepository;
        }

        public async Task<KeycloakResponse> LoginAsync(LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Invalid login request parameters");
            }

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
                throw new HttpRequestException($"Keycloak login failed with status {response.StatusCode}: {error}");
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<KeycloakResponse>(tokenResponse)
                ?? throw new InvalidOperationException("Failed to deserialize token response");

            return result;
        }

        public async Task<KeycloakResponse> RegisterAsync(RegisterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Email))
            {
                throw new ArgumentException("Invalid register request parameters");
            }

            // 1. گرفتن توکن ادمین
            var adminToken = await GetAdminToken();

            // 2. آماده کردن کاربر
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // 3. ایجاد کاربر
            var response = await _httpClient.PostAsync(
                $"{_settings.AuthServerUrl}/admin/realms/{_settings.Realm}/users",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to create user with status {response.StatusCode}: {error}");
            }

            // 4. گرفتن KeycloakId
            var keycloakId = await GetUserIdByUsername(request.Username, adminToken);

            //5. ثبت در دیتابیس SQL
            var newUser = new User
            {
                Username=request.Username,
                PasswordHash=PasswordGenerator.HashGenerator(request.Password),
                KeycloakId=keycloakId,
                RefreshToken=adminToken,
                CreatedAt=DateTime.Now,
                Role= 2,
                UpdatedAt=DateTime.MinValue
            };
            await _userRepository.InsertAsync(newUser);
            await _userRepository.SaveChangesAsync();

            // 6. ورود کاربر جدید
            _httpClient.DefaultRequestHeaders.Authorization = null; // حذف توکن ادمین
            var loginResponse = await LoginAsync(new LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            });

            loginResponse.KeycloakId = keycloakId;
            return loginResponse;
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
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get admin token with status {response.StatusCode}: {error}");
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<KeycloakResponse>(tokenResponse)
                ?? throw new InvalidOperationException("Failed to deserialize admin token response");

            return token.AccessToken ?? throw new InvalidOperationException("Admin token is null");
        }

        private async Task<string> GetUserIdByUsername(string username, string adminToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _httpClient.GetAsync(
                $"{_settings.AuthServerUrl}/admin/realms/{_settings.Realm}/users?username={username}"
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get user ID with status {response.StatusCode}: {error}");
            }

            var users = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(await response.Content.ReadAsStringAsync())
                ?? throw new InvalidOperationException("Failed to deserialize users response");

            var user = users.FirstOrDefault(u => u.ContainsKey("username") && u["username"].ToString() == username)
                ?? throw new InvalidOperationException($"User with username {username} not found");

            return user["id"]?.ToString() ?? throw new InvalidOperationException("User ID not found");
        }
    }
}
