using EnterpriseAutomation.Application.Permissions.DTOs;
using EnterpriseAutomation.Application.Permissions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionsController(IPermissionService service)
    {
        _service = service;
    }

    // محافظت: فقط ادمین (برای بوت‌استرپ ساده)
    [HttpPost("upsert")]
    //[Authorize(Roles = "admin")]
    public async Task<ActionResult<PermissionListItemDto>> Upsert([FromBody] PermissionUpsertDto dto, CancellationToken ct)
    {
        var result = await _service.UpsertAsync(dto, ct);
        return Ok(result);
    }

    [HttpGet]
    //[Authorize(Roles = "admin")]
    public async Task<ActionResult<IReadOnlyList<PermissionListItemDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    //[Authorize(Roles = "admin")]
    public async Task<ActionResult<PermissionListItemDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    //[Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
