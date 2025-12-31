namespace AutoBackupZipOneDrive.Notify
{
    public interface INotifyChannel
    {
        /// <summary>
        /// 发送通知（内容完全由调用方决定）
        /// </summary>
        void Notify(string text);
    }
}
