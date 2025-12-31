using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;

namespace AutoBackupZipOneDrive.Notify
{
    public class DingTalkNotifyChannel : INotifyChannel
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _webhook;
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

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