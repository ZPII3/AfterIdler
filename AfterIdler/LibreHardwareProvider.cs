using LibreHardwareMonitor.Hardware;
using System;
using System.Diagnostics;

namespace AfterIdler.Hardware
{

    public sealed class LibreHardwareProvider : ITemperatureProvider
    {
        private readonly Computer _computer;

        public string Name => "LibreHardwareMonitor";

        public LibreHardwareProvider()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true
            };
        }
            
        public bool Initialize()
        {
            try
            {
                Debug.WriteLine("");
                Debug.WriteLine("========================================");
                Debug.WriteLine("LibreHardwareMonitor Initialize");
                Debug.WriteLine("========================================");

                _computer.Open();

                foreach (IHardware hardware in _computer.Hardware)
                {
                    hardware.Update();

                    Debug.WriteLine(
                        $"Detected : {hardware.HardwareType} - {hardware.Name}");
                }

                Debug.WriteLine("Initialization Complete");
                Debug.WriteLine("");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LHM Initialize Failed");
                Debug.WriteLine(ex);

                return false;
            }
        }

    
        public void Shutdown()
        {
            try
            {
                _computer.Close();
                Debug.WriteLine("LibreHardwareMonitor Closed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public TemperatureResult Update()
        {
            TemperatureResult result = new TemperatureResult();

            foreach (IHardware hardware in _computer.Hardware)
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (!sensor.Value.HasValue)
                        continue;

                    float temp = sensor.Value.Value;

                    if (temp <= 1)
                        continue;

                    // ----- CPU -----
                    if (hardware.HardwareType == HardwareType.Cpu &&
                        sensor.SensorType == SensorType.Temperature)
                    {
                        string name = sensor.Name;

                        // AMD
                        if (name.Contains("Tctl/Tdie"))
                        {
                            result.CpuTemperature = temp;
                            continue;
                        }

                        // Intelなど
                        if (!result.CpuAvailable &&
                            (name.Contains("Package") ||
                             name.Contains("CPU Package")))
                        {
                            result.CpuTemperature = temp;
                            continue;
                        }

                        // CCD
                        if (!result.CpuAvailable &&
                            name.Contains("CCD"))
                        {
                            result.CpuTemperature = temp;
                        }
                    }

                    // ----- GPU -----
                    if (hardware.HardwareType == HardwareType.GpuNvidia ||
                        hardware.HardwareType == HardwareType.GpuAmd ||
                        hardware.HardwareType == HardwareType.GpuIntel)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            if (sensor.Name.Contains("GPU Core"))
                            {
                                result.GpuTemperature = sensor.Value.Value;
                            }
                        }
                    }
                }
            }
            #if DEBUG
                Debug.WriteLine(result);
            #endif
            return result;
        }
        public void Dispose()
        {
            Shutdown();
        }
    }
}