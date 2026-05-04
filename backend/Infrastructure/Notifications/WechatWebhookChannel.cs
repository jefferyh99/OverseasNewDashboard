using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OpsMonitor.Infrastructure.Notifications;

/// <summary>
/// 企业微信群机器人 Webhook 推送。
/// 配置：Notifications:WechatWebhook:Enabled=true, WebhookUrl=https://qyapi.weixin.qq.com/...
/// </summary>
public sealed class WechatWebhookChannel(
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<WechatWebhookChannel> logger) : INotificationChannel
{
    public string ChannelCode => "wechat";

    public async Task<bool> SendAsync(NotificationMessage msg, CancellationToken ct = default)
    {
        var enabled = config.GetValue<bool>("Notifications:WechatWebhook:Enabled");
        if (!enabled) return true; // 未启用时静默成功

        var webhookUrl = config["Notifications:WechatWebhook:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.LogWarning("[WechatWebhook] WebhookUrl 未配置，跳过发送。");
            return false;
        }

        var content = BuildMarkdown(msg);
        var payload = new { msgtype = "markdown", markdown = new { content } };

        try
        {
            var client = httpClientFactory.CreateClient("wechat");
            var response = await client.PostAsJsonAsync(webhookUrl, payload, ct);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("[WechatWebhook] 发送成功：{Title}", msg.Title);
                return true;
            }
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("[WechatWebhook] 发送失败 {Status}：{Body}", response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[WechatWebhook] 发送异常");
            return false;
        }
    }

    private static string BuildMarkdown(NotificationMessage msg)
    {
        return $"""
            ## 🔔 {msg.Title}
            **仓库**：{msg.WarehouseName}　　**时间**：{DateTimeOffset.Now:yyyy-MM-dd HH:mm}

            | 类型 | 数量 |
            |------|------|
            | 即将超时 | <font color="warning">{msg.ImminentCount}</font> |
            | 已超时 | <font color="warning">{msg.OverdueCount}</font> |

            {msg.Body}
            """;
    }
}
