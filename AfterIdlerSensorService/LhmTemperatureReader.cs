using LibreHardwareMonitor.Hardware;
using System.Diagnostics;

namespace AfterIdlerSensorService;

public sealed class LhmTemperatureReader : IDisposable
{
    private Computer? _computer;

    private float? _cpuTemperature;
    private float? _gpuTemperature;

    private readonly object _lock = new();

    public void Initialize()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true
        };

        _computer.Open();

        Debug.WriteLine(
            "LibreHardwareMonitor initialized.");

        DumpHardwareTree();
    }

    public void Update()
    {
        if (_computer == null)
            return;

        float? cpu = null;
        float? gpu = null;

        foreach (IHardware hardware in _computer.Hardware)
        {
            UpdateHardware(
                hardware,
                ref cpu,
                ref gpu);
        }

        lock (_lock)
        {
            _cpuTemperature = cpu;
            _gpuTemperature = gpu;
        }
    }

    private static void UpdateHardware(
        IHardware hardware,
        ref float? cpu,
        ref float? gpu)
    {
        hardware.Update();

        foreach (ISensor sensor in hardware.Sensors)
        {
            if (!sensor.Value.HasValue)
                continue;

            float value = sensor.Value.Value;

            // 0以下は無効値として扱う
            if (value <= 0)
                continue;

            if (sensor.SensorType != SensorType.Temperature)
                continue;

            if (hardware.HardwareType == HardwareType.Cpu)
            {
                if (sensor.Name.Contains(
                    "Tctl/Tdie",
                    StringComparison.OrdinalIgnoreCase))
                {
                    cpu = value;
                    continue;
                }

                if (cpu == null &&
                    sensor.Name.Contains(
                        "Package",
                        StringComparison.OrdinalIgnoreCase))
                {
                    cpu = value;
                    continue;
                }

                if (cpu == null &&
                    sensor.Name.Contains(
                        "CCD",
                        StringComparison.OrdinalIgnoreCase))
                {
                    cpu = value;
                }
            }

            if (hardware.HardwareType == HardwareType.GpuNvidia ||
                hardware.HardwareType == HardwareType.GpuAmd ||
                hardware.HardwareType == HardwareType.GpuIntel)
            {
                if (sensor.Name.Contains(
                    "GPU Core",
                    StringComparison.OrdinalIgnoreCase))
                {
                    gpu = value;
                }
            }
        }

        foreach (IHardware subHardware
            in hardware.SubHardware)
        {
            UpdateHardware(
                subHardware,
                ref cpu,
                ref gpu);
        }
    }

    public string GetResponse()
    {
        lock (_lock)
        {
            string cpu =
                _cpuTemperature.HasValue
                    ? _cpuTemperature.Value.ToString("F1")
                    : "--";

            string gpu =
                _gpuTemperature.HasValue
                    ? _gpuTemperature.Value.ToString("F1")
                    : "--";

            return $"CPU={cpu};GPU={gpu}";
        }
    }

    private void DumpHardwareTree()
    {
        if (_computer == null)
            return;

        var lines = new List<string>();

        lines.Add("AfterIdler temperature probe result");
        lines.Add("-----------------------------------");

        foreach (IHardware hardware
            in _computer.Hardware)
        {
            DumpHardware(
                hardware,
                0,
                lines);
        }

        DumpTemperatureTargets(lines);

        EventLog.WriteEntry(
            "AfterIdlerSensorService",
            string.Join(Environment.NewLine, lines),
            EventLogEntryType.Information);
    }

    private void DumpTemperatureTargets(
    List<string> lines)
    {
        if (_computer == null)
            return;

        float? cpu = null;
        string? cpuName = null;

        float? gpu = null;
        string? gpuName = null;

        foreach (IHardware hardware
            in _computer.Hardware)
        {
            FindTemperatureTargets(
                hardware,
                ref cpu,
                ref cpuName,
                ref gpu,
                ref gpuName);
        }

        lines.Add("");
        lines.Add("Selected temperature targets:");

        if (cpu.HasValue)
        {
            lines.Add(
                $"  CPU = {cpuName} = {cpu.Value:F1} C");
        }
        else
        {
            lines.Add("  CPU = --");
        }

        if (gpu.HasValue)
        {
            lines.Add(
                $"  GPU = {gpuName} = {gpu.Value:F1} C");
        }
        else
        {
            lines.Add("  GPU = --");
        }
    }

    private static void FindTemperatureTargets(
    IHardware hardware,
    ref float? cpu,
    ref string? cpuName,
    ref float? gpu,
    ref string? gpuName)
    {
        hardware.Update();

        foreach (ISensor sensor
            in hardware.Sensors)
        {
            if (sensor.SensorType != SensorType.Temperature)
                continue;

            if (!sensor.Value.HasValue)
                continue;

            float value = sensor.Value.Value;

            if (value <= 0)
                continue;

            // CPU
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                if (cpu == null &&
                    sensor.Name.Contains(
                        "Tctl/Tdie",
                        StringComparison.OrdinalIgnoreCase))
                {
                    cpu = value;
                    cpuName = sensor.Name;
                }
                else if (cpu == null &&
                         sensor.Name.Equals(
                             "CPU Package",
                             StringComparison.OrdinalIgnoreCase))
                {
                    cpu = value;
                    cpuName = sensor.Name;
                }
            }

            // GPU
            if (hardware.HardwareType == HardwareType.GpuNvidia ||
                hardware.HardwareType == HardwareType.GpuAmd ||
                hardware.HardwareType == HardwareType.GpuIntel)
            {
                if (gpu == null &&
                    sensor.Name.Equals(
                        "GPU Core",
                        StringComparison.OrdinalIgnoreCase))
                {
                    gpu = value;
                    gpuName = sensor.Name;
                }
            }
        }

        foreach (IHardware subHardware
            in hardware.SubHardware)
        {
            FindTemperatureTargets(
                subHardware,
                ref cpu,
                ref cpuName,
                ref gpu,
                ref gpuName);
        }
    }

    private static void DumpHardware(
        IHardware hardware,
        int depth,
        List<string> lines)
    {
        string indent =
            new string(' ', depth * 2);

        lines.Add(
            $"{indent}{hardware.HardwareType}: {hardware.Name}");

        foreach (ISensor sensor
            in hardware.Sensors)
        {
            if (sensor.SensorType != SensorType.Temperature)
                continue;

            lines.Add(
                $"{indent}  Temperature: " +
                $"{sensor.Name} = " +
                $"{sensor.Value}");
        }

        foreach (IHardware subHardware
            in hardware.SubHardware)
        {
            DumpHardware(
                subHardware,
                depth + 1,
                lines);
        }
    }

    public void Dispose()
    {
        try
        {
            _computer?.Close();
        }
        catch
        {
        }

        _computer = null;
    }
}