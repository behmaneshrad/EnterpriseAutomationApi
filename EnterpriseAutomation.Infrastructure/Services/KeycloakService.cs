using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Infrastructure.Services
{
    public class KeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public KeycloakService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var keycloakConfig = _configuration.GetSection("Keycloak");
            // Use admin client for admin operations if available
            var clientId = keycloakConfig["AdminClientId"] ?? keycloakConfig["ClientId"];
            var clientSecret = keycloakConfig["AdminClientSecret"] ?? keycloakConfig["ClientSecret"];
            var realm = keycloakConfig["Realm"];
            var authority = keycloakConfig["Authority"];

            var tokenUrl = $"{authority}/realms/{realm}/protocol/openid-connect/token";

            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            var content = new FormUrlEncodedContent(parameters);

            try
            {
                var response = await _httpClient.PostAsync(tokenUrl, content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Keycloak Token Error: {response.StatusCode} - {body}");

                using var doc = JsonDocument.Parse(body);
                return doc.RootElement.GetProperty("access_token").GetString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get access token: {ex.Message}", ex);
            }
        }

        public async Task<string> GetUsersAsync()
        {
            var token = await GetAccessTokenAsync();
            var keycloakConfig = _configuration.GetSection("Keycloak");
            var authority = keycloakConfig["Authority"];
            var realm = keycloakConfig["Realm"];
            var url = $"{authority}/admin/realms/{realm}/users";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetRolesAsync()
        {
            var token = await GetAccessTokenAsync();
            var keycloakConfig = _configuration.GetSection("Keycloak");
            var authority = keycloakConfig["Authority"];
            var realm = keycloakConfig["Realm"];
            var url = $"{authority}/admin/realms/{realm}/roles";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> CreateUserAsync(string username, string email, string password, string firstName = "", string lastName = "")
        {
            try
            {
                var token = await GetAccessTokenAsync();
                Console.WriteLine("ACCESS TOKEN:\n" + token);
                var keycloakConfig = _configuration.GetSection("Keycloak");
                var authority = keycloakConfig["Authority"];
                var realm = keycloakConfig["Realm"];
                var url = $"{authority}/admin/realms/{realm}/users";

                var userPayload = new
                {
                    username = username,
                    email = email,
                    firstName = firstName,
                    lastName = lastName,
                    enabled = true,
                    emailVerified = true,
                    credentials = new[]
                    {
                new { type = "password", value = password, temporary = false }
            }
                };

                var json = JsonSerializer.Serialize(userPayload);
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Debug information
                Console.WriteLine($"Request URL: {url}");
                if (!string.IsNullOrEmpty(token))
                    Console.WriteLine($"Token (first 20 chars): {token.Substring(0, Math.Min(20, token.Length))}...");
                else
                    Console.WriteLine("Token is null or empty.");
                Console.WriteLine($"Payload: {json}");

                var response = await _httpClient.SendAsync(request);
                Console.WriteLine("Request : \n" + request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var headers = response.Headers.ToString();
                    throw new Exception($"Keycloak user creation failed: {response.StatusCode} - {errorContent}. Headers: {headers}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create user: {ex.Message}", ex);
            }
        }

        public async Task<string> LoginUserAsync(string username, string password)
        {
            try
            {
                var keycloakConfig = _configuration.GetSection("Keycloak");
                var authority = keycloakConfig["Authority"];
                var realm = keycloakConfig["Realm"];
                var clientId = keycloakConfig["ClientId"];
                var clientSecret = keycloakConfig["ClientSecret"];

                var tokenUrl = $"{authority}/realms/{realm}/protocol/openid-connect/token";

                var parameters = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password },
                    { "client_secret", clientSecret }
                };

                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(tokenUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Login failed: {response.StatusCode} - {responseContent}");
                }

                return responseContent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to login user: {ex.Message}", ex);
            }
        }
    }
}