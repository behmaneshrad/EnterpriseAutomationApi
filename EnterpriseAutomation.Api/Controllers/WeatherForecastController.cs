using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public() => Ok("عمومی: بدون توکن");

    [Authorize]
    [HttpGet("private")]
    public IActionResult Private() => Ok("خصوصی: نیاز به توکن");

    [Authorize(Roles = "admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly() => Ok("فقط ادمین!");
}
