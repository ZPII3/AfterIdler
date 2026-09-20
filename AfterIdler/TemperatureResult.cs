using System;

namespace AfterIdler.Hardware
{

    /// 温度取得結果
    /// </summary>
    public sealed class TemperatureResult
    {
    
        /// CPU温度（取得失敗時はnull）
        /// </summary>
        public float? CpuTemperature { get; set; }

    
        /// GPU温度（取得失敗時はnull）
        /// </summary>
        public float? GpuTemperature { get; set; }

    
        /// CPU温度取得可否
        /// </summary>
        public bool CpuAvailable => CpuTemperature.HasValue;

    
        /// GPU温度取得可否
        /// </summary>
        public bool GpuAvailable => GpuTemperature.HasValue;

    
        /// CPU・GPUどちらか取得できているか
        /// </summary>
        public bool HasAnyTemperature => CpuAvailable || GpuAvailable;

    
        /// 値をリセット
        /// </summary>
        public void Clear()
        {
            CpuTemperature = null;
            GpuTemperature = null;
        }

    
        /// デバッグ表示用
        /// </summary>
        public override string ToString()
        {
            string cpu = CpuAvailable
                ? $"{CpuTemperature:F1}°C"
                : "--";

            string gpu = GpuAvailable
                ? $"{GpuTemperature:F1}°C"
                : "--";

            return $"CPU={cpu}, GPU={gpu}";
        }
    }
}