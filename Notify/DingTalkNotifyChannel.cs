using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace AutoBackupZipOneDrive.Notify
{
    public class DingTalkNotifyChannel : INotifyChannel
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _webhook;

        public DingTalkNotifyChannel(string webhook)
        {
            _webhook = webhook;
        }

        public void Notify(string text)
        {
            var payload = new
            {
                msgtype = "text",
                text = new
                {
                    content = text
                }
            };

            string json = JsonConvert.SerializeObject(payload);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = _httpClient
                .PostAsync(_webhook, content)
                .GetAwaiter()
                .GetResult();

            response.EnsureSuccessStatusCode();
        }
    }
}
