using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseAutomation.Infrastructure.Services
{
    public class KeycloakOptions
    {
        public string Authority { get; set; } = default!;     // مثل: http://localhost:8080
        public string Realm { get; set; } = default!;         // مثل: EnterpriseRealm

        // برای ورود کاربر (Password Grant) و اعتبارسنجی توکن‌ها
        public string ClientId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;

        // برای عملیات ادمینی (Admin REST API):
        public string AdminClientId { get; set; } = default!;
        public string AdminClientSecret { get; set; } = default!;
    }

    public class KeycloakService
    {
        private readonly HttpClient _http;
        private readonly KeycloakOptions _opt;
        private readonly ILogger<KeycloakService>? _logger;

        // کش بسیار ساده‌ی توکن ادمین (برای کاهش درخواست‌های اضافی)
        private string? _adminToken;
        private DateTimeOffset _adminTokenExpiresAt = DateTimeOffset.MinValue;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public KeycloakService(HttpClient httpClient, IOptions<KeycloakOptions> options, ILogger<KeycloakService>? logger = null)
        {
            _http = httpClient;
            _opt = options.Value;
            _logger = logger;
            if (!string.IsNullOrWhiteSpace(_opt.Authority))
            {
                _http.BaseAddress = new Uri(_opt.Authority.TrimEnd('/') + "/");
            }
        }

        // ====== Helpers ======
        private static Exception HttpError(string action, HttpResponseMessage resp, string body) =>
            new($"Keycloak {action} failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");

        private static string Combine(params string[] parts)
        {
            // ساده و امن برای ساخت URL نسبی
            return string.Join("/", parts.Select(p => p.Trim('/')));
        }

        // ====== Admin Token (Client Credentials) ======
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
            var req = new HttpRequestMessage(method, relativeUrl);
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

        public async Task<bool> CreateUserAsync(string username, string email, string password, string firstName = "", string lastName = "", string defaultRealmRoleToAssign = "employee", CancellationToken ct = default)
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

            // userId از Location header
            string? userId = resp.Headers.Location?.Segments.LastOrDefault()?.Trim('/');
            if (string.IsNullOrEmpty(userId))
            {
                // اگر Location نبود، با username جست‌وجو کن
                userId = await GetUserIdByUsernameAsync(username, ct);
                if (string.IsNullOrEmpty(userId)) throw new("User created but ID could not be resolved.");
            }

            // یک نقش Realm پیش‌فرض بده (اختیاری)
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

            try
            {
                var users = JsonSerializer.Deserialize<List<JsonElement>>(body);
                if (users is { Count: > 0 })
                {
                    return users[0].TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new($"Parse users response failed: {ex.Message}", ex);
            }
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
            // 1) نقش Realm را بگیر
            var role = await GetRealmRoleAsync(roleName, ct)
                ?? throw new($"Realm role '{roleName}' not found.");

            // 2) نقش را به کاربر بده
            var url = Combine("admin/realms", _opt.Realm, "users", userId, "role-mappings/realm");
            var payload = JsonSerializer.Serialize(new[] { role }, JsonOpts);

            var req = await AuthedAdminRequestAsync(HttpMethod.Post, url,
                new StringContent(payload, Encoding.UTF8, "application/json"), ct);

            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("AssignRealmRole", resp, body);

            return true;
        }

        public async Task CheckUserRealmRolesAsync(string userId, CancellationToken ct = default)
        {
            var url = Combine("admin/realms", _opt.Realm, "users", userId, "role-mappings/realm");
            var req = await AuthedAdminRequestAsync(HttpMethod.Get, url, null, ct);
            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw HttpError("GetUserRealmRoles", resp, body);
            _logger?.LogInformation("User {UserId} realm roles: {Body}", userId, body);
        }

        private async Task<RoleDto?> GetRealmRoleAsync(string roleName, CancellationToken ct = default)
        {
            var url = Combine("admin/realms", _opt.Realm, "roles", Uri.EscapeDataString(roleName));
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

        // ====== LOGIN (Resource Owner Password) ======
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

            return body; // شامل access_token, refresh_token, ...
        }

        public async Task CreateRealmRoleAsync(string name, string? description, CancellationToken ct)
        {
            throw new NotImplementedException();
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
