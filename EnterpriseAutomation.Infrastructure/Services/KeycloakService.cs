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
            var tokenUrl = $"{keycloakConfig["Authority"]}/protocol/openid-connect/token";

            var clientId = keycloakConfig["ClientId"];
            var clientSecret = keycloakConfig["ClientSecret"];

            var content = new StringContent(
                $"client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials",
                Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync(tokenUrl, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonDocument.Parse(json);

            return tokenResponse.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<string> GetUsersAsync()
        {
            var token = await GetAccessTokenAsync();
            var keycloakConfig = _configuration.GetSection("Keycloak");
            var url = $"{keycloakConfig["Authority"]}/admin/realms/{keycloakConfig["Realm"]}/users";

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
            var url = $"{keycloakConfig["Authority"]}/admin/realms/{keycloakConfig["Realm"]}/roles";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
