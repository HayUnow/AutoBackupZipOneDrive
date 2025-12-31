using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;

namespace AutoBackupZipOneDrive.Notify
{
    public class TelegramNotifyChannel : INotifyChannel
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        private readonly string _botToken;
        private readonly string _chatId;

        public TelegramNotifyChannel(string botToken, string chatId)
        {
            _botToken = botToken;
            _chatId = chatId;
        }

        public void Notify(string text)
        {
            var payload = new
            {
                chat_id = _chatId,
                text = text
            };

            string json = _json.Serialize(payload);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                string url = "https://api.telegram.org/bot"
                             + _botToken
                             + "/sendMessage";

                var response = _httpClient
                    .PostAsync(url, content)
                    .GetAwaiter()
                    .GetResult();

                response.EnsureSuccessStatusCode();
            }
        }
    }
}