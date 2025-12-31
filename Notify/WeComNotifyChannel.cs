using AutoBackupZipOneDrive.Core;

namespace AutoBackupZipOneDrive.Notify
{
    public class WeComNotifyChannel : INotifyChannel
    {
        private readonly WeComWebhookNotifier _inner;

        public WeComNotifyChannel(WeComWebhookNotifier inner)
        {
            _inner = inner;
        }

        public void Notify(string text)
        {
            // 行为和你原来一模一样
            _ = _inner.SendTextAsync(text);
        }
    }
}
