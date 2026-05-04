using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpsMonitor.Infrastructure.Notifications;

namespace OpsMonitor.Api.BackgroundServices;

/// <summary>定时扫描异常对象并触发提醒推送。</summary>
public sealed class ReminderScanHostedService(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<ReminderScanHostedService> logger) : BackgroundService
{
    private readonly int _intervalMinutes =
        config.GetValue<int>("Notifications:ScanIntervalMinutes", 5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[ReminderScan] 后台服务已启动，扫描间隔 {Min} 分钟。", _intervalMinutes);

        // 启动后稍等 30 秒再首次执行，避免与应用启动竞争
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<ReminderDispatchService>();
                await svc.ScanAndSendAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[ReminderScan] 扫描异常，将在下一轮重试。");
            }

            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }

        logger.LogInformation("[ReminderScan] 后台服务已停止。");
    }
}
