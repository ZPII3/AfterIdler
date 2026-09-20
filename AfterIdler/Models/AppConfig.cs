using System;

namespace AfterIdler.Models
{

    /// アプリケーション設定
    /// </summary>
    public class AppConfig
    {
        public int TargetCpuTemp { get; set; } = 45;

        public int TargetGpuTemp { get; set; } = 40;

        public int TargetMinutes { get; set; } = 3;

        public string EndMode { get; set; } = "OFF";

        public double WindowLeft { get; set; } = -1;

        public double WindowTop { get; set; } = -1;

        public AppConfig Clone()
        {
            return (AppConfig)MemberwiseClone();
        }
    }
}