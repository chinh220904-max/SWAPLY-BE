using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Swaply.Application.ReportManagement;
using Swaply.Application.ReportManagement.FluentValidation;
using Swaply.Domain.Repositories;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/admin/reports")]
[Authorize]
public class AdminReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly IUserRepository _userRepository;
    private readonly ProcessReportValidator _processReportValidator;

    public AdminReportsController(
        IReportService reportService,
        IUserRepository userRepository,
        ProcessReportValidator processReportValidator)
    {
        _reportService = reportService;
        _userRepository = userRepository;
        _processReportValidator = processReportValidator;
    }

    private async Task<bool> IsAdminAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        var role = await _userRepository.GetRoleByNameAsync("Admin");
        return role != null && user.RoleId == role.Id;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReports([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var result = await _reportService.GetAllReportsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingReports([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var result = await _reportService.GetPendingReportsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReportById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        try
        {
            var report = await _reportService.GetReportByIdAsync(id);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> ApproveReport(Guid id, [FromBody] ProcessReportRequest? request)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        request ??= new ProcessReportRequest("Đã duyệt báo cáo");

        var validation = await _processReportValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { message = validation.Errors[0].ErrorMessage });

        try
        {
            var report = await _reportService.ApproveReportAsync(id, request.AdminNote);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> RejectReport(Guid id, [FromBody] ProcessReportRequest? request)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        request ??= new ProcessReportRequest("Đã bác bỏ báo cáo");

        var validation = await _processReportValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { message = validation.Errors[0].ErrorMessage });

        try
        {
            var report = await _reportService.RejectReportAsync(id, request.AdminNote);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
