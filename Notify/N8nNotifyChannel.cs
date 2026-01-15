using System;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;

namespace AutoBackupZipOneDrive.Notify
{
    /// <summary>
    /// 统一推送到 n8n Webhook
    /// n8n 再负责转发：钉钉/企微/TG 等
    /// </summary>
    public class N8nNotifyChannel : INotifyChannel
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        private readonly string _webhook;

        public N8nNotifyChannel(string webhook)
        {
            _webhook = webhook?.Trim();

            if (string.IsNullOrWhiteSpace(_webhook))
                throw new ArgumentException("n8n webhook 不能为空", nameof(webhook));
        }

        public void Notify(string text)
        {
            // ✅ 推荐用结构化 JSON，后续你在 n8n 里能做更多判断/分流/美化
            var payload = new
            {
                source = "AutoBackupZipOneDrive",
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                message = text //这里就是要发送的内容文本格式
            };

            string json = _json.Serialize(payload);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = _httpClient
                    .PostAsync(_webhook, content)
                    .GetAwaiter()
                    .GetResult();

                response.EnsureSuccessStatusCode();
            }
        }
    }
}
