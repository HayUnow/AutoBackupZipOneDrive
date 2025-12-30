using System;

public class FileCandidate
{
    public string Path { get; set; }

    // 最近一次观测到的大小
    public long Size { get; set; }

    // 最近一次写入时间（仅参考）
    public DateTime WriteTime { get; set; }

    // 从“尺寸不再变化”开始计时
    public DateTime StableSince { get; set; }

    // 连续稳定轮次
    public int StableRounds { get; set; }

    // 是否最终确认完成
    public bool IsStable { get; set; }

    // 当前状态描述（UI 用）
    public string StateText { get; set; }
    //历史文件
    public bool IsHistory { get; set; }

}
