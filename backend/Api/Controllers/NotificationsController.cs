using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpsMonitor.Application.Notifications;
using OpsMonitor.Infrastructure.Notifications;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Notifications;
using OpsMonitor.Infrastructure.Persistence;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(
    ReminderDispatchService dispatchService,
    OpsMonitorDbContext db) : ControllerBase
{
    /// <summary>查看最近 50 条提醒日志。</summary>
    [HttpGet("reminders")]
    public async Task<IActionResult> GetReminders()
    {
        var logs = await db.Set<OpsMonitor.Domain.ReminderLog>()
            .OrderByDescending(r => r.SentAt)
            .Take(50)
            .Select(r => new ReminderLogDto(
                r.Id, r.ObjectType, r.ObjectId, r.EventType,
                r.Channel, r.SentAt, r.Success, r.ErrorMessage))
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<ReminderLogDto>>.Ok(logs));
    }

    /// <summary>手动立即触发一次扫描（用于测试）。</summary>
    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger()
    {
        var before = await db.Set<OpsMonitor.Domain.ReminderLog>().CountAsync();
        await dispatchService.ScanAndSendAsync();
        var after = await db.Set<OpsMonitor.Domain.ReminderLog>().CountAsync();

        var resp = new TriggerTestResponse("扫描完成", after - before);
        return Ok(ApiResponse<TriggerTestResponse>.Ok(resp));
    }
}
