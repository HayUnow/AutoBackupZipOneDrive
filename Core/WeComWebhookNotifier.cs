using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoBackupZipOneDrive.Core
{
    /// <summary>
    /// 企业微信群 Webhook 通知封装（兼容 C# 7.3）
    /// </summary>
    public class WeComWebhookNotifier
    {
        private readonly string _webhook;

        public WeComWebhookNotifier(string webhook)
        {
            _webhook = webhook == null ? null : webhook.Trim();
        }

        public bool IsEnabled
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_webhook)
                    && _webhook.StartsWith(
                        "https://qyapi.weixin.qq.com/cgi-bin/webhook/send",
                        StringComparison.OrdinalIgnoreCase);
            }
        }

        public Task SendTextAsync(string content)
        {
            if (!IsEnabled)
                return Task.CompletedTask;

            if (string.IsNullOrWhiteSpace(content))
                return Task.CompletedTask;

            return SendInternalAsync(content);
        }

        private async Task SendInternalAsync(string content)
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                string json =
                    "{"
                    + "\"msgtype\":\"text\","
                    + "\"text\":{"
                    + "\"content\":\"" + Escape(content) + "\""
                    + "}"
                    + "}";

                var httpContent = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                await client.PostAsync(_webhook, httpContent);
            }
            catch
            {
                // 通知失败不影响主流程
            }
        }

        private static string Escape(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "")
                .Replace("\n", "\\n");
        }
    }
}