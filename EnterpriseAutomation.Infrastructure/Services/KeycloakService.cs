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

                // Get the created user's ID from the Location header
                var locationHeader = response.Headers.Location?.ToString();
                if (string.IsNullOrEmpty(locationHeader))
                {
                    // If location header is not available, try to find user by username
                    Console.WriteLine("Location header not found, searching for user by username...");
                    var userId = await GetUserIdByUsernameAsync(username);
                    if (string.IsNullOrEmpty(userId))
                    {
                        throw new Exception("Failed to get user ID from response headers and couldn't find user by username");
                    }

                    Console.WriteLine($"Found user ID by username search: {userId}");
                    await AssignRoleToUserAsync(userId, "employee");
                }
                else
                {
                    var userId = locationHeader.Split('/').Last();
                    Console.WriteLine($"Created user ID: {userId}");
                    await AssignRoleToUserAsync(userId, "employee");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create user: {ex.Message}", ex);
            }
        }

        public async Task<string> GetUserIdByUsernameAsync(string username)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var keycloakConfig = _configuration.GetSection("Keycloak");
                var authority = keycloakConfig["Authority"];
                var realm = keycloakConfig["Realm"];
                var url = $"{authority}/admin/realms/{realm}/users?username={username}&exact=true";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to search for user: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<JsonElement[]>(responseContent);

                if (users != null && users.Length > 0)
                {
                    return users[0].GetProperty("id").GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching for user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var keycloakConfig = _configuration.GetSection("Keycloak");
                var authority = keycloakConfig["Authority"];
                var realm = keycloakConfig["Realm"];

                // First, get the role details
                var role = await GetRealmRoleAsync(roleName);
                if (role == null)
                {
                    throw new Exception($"Role '{roleName}' not found in realm");
                }

                // Assign the role to the user
                var roleAssignmentUrl = $"{authority}/admin/realms/{realm}/users/{userId}/role-mappings/realm";
                var rolePayload = new[] { role };
                var roleJson = JsonSerializer.Serialize(rolePayload);

                var roleRequest = new HttpRequestMessage(HttpMethod.Post, roleAssignmentUrl)
                {
                    Content = new StringContent(roleJson, Encoding.UTF8, "application/json")
                };
                roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine($"Assigning role '{roleName}' to user {userId}");
                Console.WriteLine($"Role assignment URL: {roleAssignmentUrl}");
                Console.WriteLine($"Role payload: {roleJson}");

                var roleResponse = await _httpClient.SendAsync(roleRequest);

                if (!roleResponse.IsSuccessStatusCode)
                {
                    var errorContent = await roleResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to assign role: {roleResponse.StatusCode} - {errorContent}");

                    // Let's also check what roles the user currently has
                    await CheckUserRolesAsync(userId);

                    throw new Exception($"Failed to assign role: {roleResponse.StatusCode} - {errorContent}");
                }

                Console.WriteLine($"Successfully assigned '{roleName}' role to user {userId}");

                // Verify the role assignment
                await CheckUserRolesAsync(userId);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to assign role to user: {ex.Message}", ex);
            }
        }

        public async Task CheckUserRolesAsync(string userId)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var keycloakConfig = _configuration.GetSection("Keycloak");
                var authority = keycloakConfig["Authority"];
                var realm = keycloakConfig["Realm"];

                var userRolesUrl = $"{authority}/admin/realms/{realm}/users/{userId}/role-mappings/realm";
                var roleRequest = new HttpRequestMessage(HttpMethod.Get, userRolesUrl);
                roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var roleResponse = await _httpClient.SendAsync(roleRequest);

                if (roleResponse.IsSuccessStatusCode)
                {
                    var rolesJson = await roleResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Current user roles: {rolesJson}");
                }
                else
                {
                    var errorContent = await roleResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to get user roles: {roleResponse.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking user roles: {ex.Message}");
            }
        }

        private async Task<RoleDto> GetRealmRoleAsync(string roleName)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var keycloakConfig = _configuration.GetSection("Keycloak");
                var authority = keycloakConfig["Authority"];
                var realm = keycloakConfig["Realm"];

                var roleUrl = $"{authority}/admin/realms/{realm}/roles/{roleName}";
                var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleUrl);
                roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var roleResponse = await _httpClient.SendAsync(roleRequest);

                if (!roleResponse.IsSuccessStatusCode)
                {
                    var errorContent = await roleResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to get role '{roleName}': {roleResponse.StatusCode} - {errorContent}");
                    return null;
                }

                var roleJson = await roleResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Role details: {roleJson}");

                var roleObject = JsonSerializer.Deserialize<JsonElement>(roleJson);

                return new RoleDto
                {
                    id = roleObject.GetProperty("id").GetString(),
                    name = roleObject.GetProperty("name").GetString()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting realm role: {ex.Message}");
                return null;
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
    //
    public class RoleDto
    {
        public required string id { get; set; }
        public required string name { get; set; }
    }
}