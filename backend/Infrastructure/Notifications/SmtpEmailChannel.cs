using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OpsMonitor.Infrastructure.Notifications;

/// <summary>
/// SMTP 邮件推送。
/// 配置：Notifications:Smtp:Enabled=true, Host, Port, Username, Password, FromAddress, ToAddresses
/// </summary>
public sealed class SmtpEmailChannel(
    IConfiguration config,
    ILogger<SmtpEmailChannel> logger) : INotificationChannel
{
    public string ChannelCode => "email";

    public async Task<bool> SendAsync(NotificationMessage msg, CancellationToken ct = default)
    {
        var enabled = config.GetValue<bool>("Notifications:Smtp:Enabled");
        if (!enabled) return true;

        var host = config["Notifications:Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning("[SmtpEmail] SMTP Host 未配置，跳过发送。");
            return false;
        }

        var port     = config.GetValue<int>("Notifications:Smtp:Port", 587);
        var username = config["Notifications:Smtp:Username"] ?? string.Empty;
        var password = config["Notifications:Smtp:Password"] ?? string.Empty;
        var from     = config["Notifications:Smtp:FromAddress"] ?? "ops@example.com";
        var fromName = config["Notifications:Smtp:FromName"] ?? "Ops Monitor";
        var toRaw    = config["Notifications:Smtp:ToAddresses"] ?? string.Empty;
        var toList   = toRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (toList.Length == 0)
        {
            logger.LogWarning("[SmtpEmail] 未配置收件人 ToAddresses，跳过发送。");
            return false;
        }

        try
        {
            using var smtp = new SmtpClient(host, port);
            smtp.EnableSsl = true;
            if (!string.IsNullOrEmpty(username))
                smtp.Credentials = new System.Net.NetworkCredential(username, password);

            using var mail = new MailMessage();
            mail.From = new MailAddress(from, fromName);
            foreach (var to in toList) mail.To.Add(to);
            mail.Subject = $"[Ops Monitor] {msg.Title}";
            mail.Body    = BuildHtmlBody(msg);
            mail.IsBodyHtml = true;

            await smtp.SendMailAsync(mail, ct);
            logger.LogInformation("[SmtpEmail] 发送成功：{Title} → {To}", msg.Title, string.Join(", ", toList));
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SmtpEmail] 发送异常");
            return false;
        }
    }

    private static string BuildHtmlBody(NotificationMessage msg) => $"""
        <h2>🔔 {msg.Title}</h2>
        <p><b>仓库：</b>{msg.WarehouseName}&nbsp;&nbsp;<b>时间：</b>{DateTimeOffset.Now:yyyy-MM-dd HH:mm}</p>
        <table border="1" cellpadding="6" style="border-collapse:collapse">
          <tr><th>即将超时</th><td style="color:orange">{msg.ImminentCount}</td></tr>
          <tr><th>已超时</th><td style="color:red">{msg.OverdueCount}</td></tr>
        </table>
        <p>{msg.Body.Replace(Environment.NewLine, "<br/>")}</p>
        """;
}
