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
    public class KeycloakOptions
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

    public class KeycloakService
    {
        private readonly HttpClient _http;
        private readonly KeycloakOptions _opt;
        private readonly ILogger<KeycloakService>? _logger;

        private string? _adminToken;
        private DateTimeOffset _adminTokenExpiresAt = DateTimeOffset.MinValue;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
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

            if (string.IsNullOrWhiteSpace(_opt.Authority))
                throw new InvalidOperationException("Keycloak 'Authority' must be set in configuration.");

            _http.BaseAddress = new Uri(_opt.Authority.TrimEnd('/') + "/");

            if (string.IsNullOrWhiteSpace(_opt.Realm))
                throw new InvalidOperationException("Keycloak 'Realm' must be set in configuration.");
        }

        private static Exception HttpError(string action, HttpResponseMessage resp, string body) =>
            new($"Keycloak {action} failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");

        private static string Combine(params string[] parts)
        {
            if (parts == null)
                return string.Empty;

            return string.Join("/", parts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim('/')));
        }

        private async Task<string> GetAdminAccessTokenAsync(CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(_adminToken) && DateTimeOffset.UtcNow < _adminTokenExpiresAt.AddSeconds(-30))
                return _adminToken!;

            var tokenUrl = Combine("realms", _opt.Realm, "protocol/openid-connect/token");

            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _opt.AdminClientId ?? _opt.ClientId,
                ["client_secret"] = _opt.AdminClientSecret ?? _opt.ClientSecret
            });

            var resp = await _http.PostAsync(tokenUrl, form, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("Token (admin)", resp, body);

            using var doc = JsonDocument.Parse(body);
            _adminToken = doc.RootElement.GetProperty("access_token").GetString();
            var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var expEl) ? expEl.GetInt32() : 60;
            _adminTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
            return _adminToken!;
        }

        private async Task<HttpRequestMessage> AuthedAdminRequestAsync(HttpMethod method, string relativeUrl, HttpContent? content = null, CancellationToken ct = default)
        {
            var token = await GetAdminAccessTokenAsync(ct);

            Uri requestUri = Uri.TryCreate(relativeUrl, UriKind.Absolute, out var absUri)
                ? absUri
                : new Uri(_http.BaseAddress!, relativeUrl);

            var req = new HttpRequestMessage(method, requestUri);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (content != null) req.Content = content;
            return req;
        }

        // ====== USERS ======
        public async Task<string> GetUsersAsync(CancellationToken ct = default)
        {
            var url = Combine("admin/realms", _opt.Realm, "users");
            var req = await AuthedAdminRequestAsync(HttpMethod.Get, url, null, ct);
            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("GetUsers", resp, body);
            return body;
        }

        public async Task<bool> CreateUserAsync(string username, string email, string password, string firstName = "", string lastName = "", string defaultRealmRoleToAssign = "NoRole", CancellationToken ct = default)
        {
            var url = Combine("admin/realms", _opt.Realm, "users");

            var payload = new
            {
                username,
                email,
                firstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName,
                lastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName,
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new { type = "password", value = password, temporary = false }
                }
            };

            var req = await AuthedAdminRequestAsync(HttpMethod.Post, url,
                new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json"), ct);

            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("CreateUser", resp, body);

            string? userId = resp.Headers.Location?.Segments.LastOrDefault()?.Trim('/');
            if (string.IsNullOrEmpty(userId))
            {
                userId = await GetUserIdByUsernameAsync(username, ct);
                if (string.IsNullOrEmpty(userId)) throw new("User created but ID could not be resolved.");
            }

            if (!string.IsNullOrWhiteSpace(defaultRealmRoleToAssign))
            {
                await AssignRealmRoleToUserAsync(userId, defaultRealmRoleToAssign, ct);
            }

            return true;
        }

        public async Task<string?> GetUserIdByUsernameAsync(string username, CancellationToken ct = default)
        {
            var url = Combine("admin/realms", _opt.Realm, $"users?username={Uri.EscapeDataString(username)}&exact=true");
            var req = await AuthedAdminRequestAsync(HttpMethod.Get, url, null, ct);
            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("SearchUser", resp, body);

            List<JsonElement>? users;
            try
            {
                users = JsonSerializer.Deserialize<List<JsonElement>>(body);
            }
            catch (Exception ex)
            {
                throw new($"Parse users response failed: {ex.Message}", ex);
            }

            if (users != null && users.Count > 0)
            {
                return users[0].TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            }

            return null;
        }

        // ====== ROLES ======
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
            var role = await GetRealmRoleAsync(roleName, ct) ?? throw new($"Realm role '{roleName}' not found.");
            var url = Combine("admin/realms", _opt.Realm, "users", userId, "role-mappings/realm");
            var payload = JsonSerializer.Serialize(new[] { role }, JsonOpts);

            var req = await AuthedAdminRequestAsync(HttpMethod.Post, url, new StringContent(payload, Encoding.UTF8, "application/json"), ct);
            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("AssignRealmRole", resp, body);

            return true;
        }

        private async Task<RoleDto?> GetRealmRoleAsync(string roleName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException("Role name is required.", nameof(roleName));

            var safeName = Uri.EscapeDataString(roleName);
            var url = Combine("admin/realms", _opt.Realm, "roles", safeName);

            var req = await AuthedAdminRequestAsync(HttpMethod.Get, url, null, ct);
            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode) return null;

            try
            {
                var el = JsonSerializer.Deserialize<JsonElement>(body);
                return new RoleDto
                {
                    Id = el.TryGetProperty("id", out var idEl) ? idEl.GetString()! : "",
                    Name = el.TryGetProperty("name", out var nameEl) ? nameEl.GetString()! : roleName
                };
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to parse realm role {RoleName}", roleName);
                return null;
            }
        }

        public async Task<string> LoginUserAsync(string username, string password, CancellationToken ct = default)
        {
            var tokenUrl = Combine("realms", _opt.Realm, "protocol/openid-connect/token");

            var form = new FormUrlEncodedContent(new Dictionary<string, string>
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

            return body;
        }

        public async Task CreateRealmRoleAsync(string name, string? description, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.", nameof(name));

            var trimmedName = name.Trim();
            var trimmedDesc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

            var existing = await GetRealmRoleAsync(trimmedName, ct);
            if (existing is not null)
            {
                _logger?.LogInformation("Realm role '{Role}' already exists. Ensuring DB persistence.", trimmedName);
                await EnsureRolePersistedAsync(trimmedName);
                return;
            }

            var url = Combine("admin/realms", _opt.Realm, "roles");
            var payloadObj = new { name = trimmedName, description = trimmedDesc };
            var content = new StringContent(JsonSerializer.Serialize(payloadObj, JsonOpts), Encoding.UTF8, "application/json");
            var req = await AuthedAdminRequestAsync(HttpMethod.Post, url, content, ct);
            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (resp.IsSuccessStatusCode)
            {
                _logger?.LogInformation("Realm role '{Role}' created in Keycloak. Persisting to DB...", trimmedName);
                await EnsureRolePersistedAsync(trimmedName);
                return;
            }

            if ((int)resp.StatusCode == 409)
            {
                _logger?.LogWarning("Realm role '{Role}' creation returned 409. Treating as success.", trimmedName);
                await EnsureRolePersistedAsync(trimmedName);
                return;
            }

            throw HttpError("CreateRealmRole", resp, body);
        }

        private async Task EnsureRolePersistedAsync(string roleName)
        {
            var exists = await _roleRepo.GetFirstOrDefaultAsync(r => r.RoleName == roleName);
            if (exists is not null) return;

            var entity = new Role { RoleName = roleName };
            await _roleRepo.InsertAsync(entity);
            await _roleRepo.SaveChangesAsync();
        }

        public async Task CreateClientRoleAsync(string clientId, string name, string? description, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }

    public class RoleDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
    }
}
