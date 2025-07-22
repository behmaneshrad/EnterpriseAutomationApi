using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EnterpriseAutomation.Application.Users.Dtos;
using EnterpriseAutomation.Infrastructure.Services;
using Newtonsoft.Json;
using EnterpriseAutomation.Application.Externals;
using System.Runtime.Serialization.Json;

namespace EnterpriseAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly KeycloakService _keycloakService;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration, KeycloakService keycloakService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _keycloakService = keycloakService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var token = await _keycloakService.GetAccessTokenAsync();
            var keycloakConfig = _configuration.GetSection("Keycloak");
            var url = $"{keycloakConfig["Authority"]}/admin/realms/{keycloakConfig["Realm"]}/users";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userPayload = new
            {
                username = model.Username,
                email = model.Email,
                enabled = true,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = model.Password,
                        temporary = false
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(userPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
                return Ok($"user created successfully");

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest($"User Creation was not successful:{error}");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var keycloakConfig = _configuration.GetSection("Keycloak");
            var tokenUrl = $"{keycloakConfig["Authority"]}/protocol/openid-connect/token";

            var content = new StringContent(
                $"client_id={keycloakConfig["ClientId"]}" +
                $"&grant_type=password" +
                $"&username={model.Username}" +
                $"&password={model.Password}" +
                $"&client_secret={keycloakConfig["ClientSecret"]}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return BadRequest(new { message = "Login failed", details = error });
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponseDto>(responseContent);

            return Ok(tokenResponse);
        }

    }
}



