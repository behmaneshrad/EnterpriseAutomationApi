using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Collections.Generic;

namespace EnterpriseAutomation.Infrastructure.Services
{
    public class KeycloakService
    {
        public string Authority { get; set; } = default!;     // e.g. http://localhost:8080
        public string Realm { get; set; } = default!;         // e.g. EnterpriseRealm

        // For user login (Password Grant) & token validation
        public string ClientId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;

        // For admin operations (Admin REST API)
        public string AdminClientId { get; set; } = default!;
        public string AdminClientSecret { get; set; } = default!;
    }

        public async Task<string> GetAccessTokenAsync()
        {
            var keycloakConfig = _configuration.GetSection("Keycloak");
            // Use admin client for admin operations if available
            var clientId = keycloakConfig["AdminClientId"] ?? keycloakConfig["ClientId"];
            var clientSecret = keycloakConfig["AdminClientSecret"] ?? keycloakConfig["ClientSecret"];
            var realm = keycloakConfig["Realm"];
            var authority = keycloakConfig["Authority"];

        // Very simple admin token cache
        private string? _adminToken;
        private DateTimeOffset _adminTokenExpiresAt = DateTimeOffset.MinValue;

            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

        private readonly IRepository<Role> _roleRepo;

        public KeycloakService(
            HttpClient httpClient,
            IOptions<KeycloakOptions> options,
            IRepository<Role> roleRepo,
            ILogger<KeycloakService>? logger = null)
        {
            _http = httpClient;
            _opt = options.Value;
            _logger = logger;
            _roleRepo = roleRepo;

            if (!string.IsNullOrWhiteSpace(_opt.Authority))
            {
                throw new Exception($"Failed to get access token: {ex.Message}", ex);
            }
        }

        public async Task<string> GetUsersAsync()
        {
            // Safe URL join for relative paths
            return string.Join("/", parts.Select(p => p.Trim('/')));
        }

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

            // userId from Location header
            string? userId = resp.Headers.Location?.Segments.LastOrDefault()?.Trim('/');
            if (string.IsNullOrEmpty(userId))
            {
                // Fallback: search by username
                userId = await GetUserIdByUsernameAsync(username, ct);
                if (string.IsNullOrEmpty(userId)) throw new("User created but ID could not be resolved.");
            }

            // Optional: assign a default realm role
            if (!string.IsNullOrWhiteSpace(defaultRealmRoleToAssign))
            {
                await AssignRealmRoleToUserAsync(userId, defaultRealmRoleToAssign, ct);
            }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create user: {ex.Message}", ex);
            }
        }

            List<JsonElement>? users;
            try
            {
                users = JsonSerializer.Deserialize<List<JsonElement>>(body);
            }
            catch (Exception ex)
            {
                throw new($"Parse users response failed: {ex.Message}", ex);
            }
        }

            return null;
        }

        // ====== ROLES (Realm) ======
        public async Task<string> GetRolesAsync(CancellationToken ct = default)
        {
            var url = Combine("admin/realms", _opt.Realm, "roles");
            var req = await AuthedAdminRequestAsync(HttpMethod.Get, url, null, ct);
            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("GetRoles", resp, body);
            return body;
        }

        public async Task<bool> AssignRealmRoleToUserAsync(string userId, string roleName, CancellationToken ct = default)
        {
            // 1) Get realm role
            var role = await GetRealmRoleAsync(roleName, ct)
                ?? throw new($"Realm role '{roleName}' not found.");

            // 2) Assign to user
            var url = Combine("admin/realms", _opt.Realm, "users", userId, "role-mappings/realm");
            var payload = JsonSerializer.Serialize(new[] { role }, JsonOpts);

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
                ["grant_type"] = "password",
                ["client_id"] = _opt.ClientId,
                ["client_secret"] = _opt.ClientSecret,
                ["username"] = username,
                ["password"] = password
            });

            var resp = await _http.PostAsync(tokenUrl, form, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("Login", resp, body);

            return body; // contains access_token, refresh_token, ...
        }

        /// <summary>
        /// Create a Keycloak realm role; then ensure it exists in local DB (idempotent in both).
        /// </summary>
        public async Task CreateRealmRoleAsync(string name, string? description, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required.", nameof(name));

            var trimmedName = name.Trim();
            var trimmedDesc = string.IsNullOrWhiteSpace(description) ? null : description!.Trim();

            // 1) If role already exists in Keycloak, ensure DB persistence and return
            var existing = await GetRealmRoleAsync(trimmedName, ct);
            if (existing is not null)
            {
                _logger?.LogInformation("Realm role '{Role}' already exists in Keycloak. Ensuring DB persistence.", trimmedName);
                await EnsureRolePersistedAsync(trimmedName);
                return;
            }

            // 2) Create role in Keycloak
            var url = Combine("admin/realms", _opt.Realm, "roles");
            var payloadObj = new
            {
                name = trimmedName,
                description = trimmedDesc
            };

                var roleObject = JsonSerializer.Deserialize<JsonElement>(roleJson);

            if (resp.IsSuccessStatusCode)
            {
                _logger?.LogInformation("Realm role '{Role}' created successfully in Keycloak. Persisting to DB...", trimmedName);
                await EnsureRolePersistedAsync(trimmedName);
                return;
            }

            // 3) If 409 (already exists, race), still persist in DB
            if ((int)resp.StatusCode == 409)
            {
                _logger?.LogWarning("Realm role '{Role}' creation returned 409. Treating as success and ensuring DB persistence.", trimmedName);
                await EnsureRolePersistedAsync(trimmedName);
                return;
            }

            // 4) Other errors
            throw HttpError("CreateRealmRole", resp, body);
        }

        public async Task CreateClientRoleAsync(string clientId, string name, string? description, CancellationToken ct)
        {
            // TODO: implement if needed
            throw new NotImplementedException();
        }
    }
    //
    public class RoleDto
    {
        public required string id { get; set; }
        public required string name { get; set; }
    }
}