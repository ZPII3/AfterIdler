using AfterIdler.Services;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;

namespace AfterIdler.Hardware
{

    public sealed class TemperatureService : IDisposable
    {
        private const string PipeName =
        "AfterIdlerSensor";

    public string ProviderName =>
        "AfterIdlerSensorService";

        public float? CpuTemperature { get; private set; }

        public float? GpuTemperature { get; private set; }

        public bool CpuAvailable =>
            CpuTemperature.HasValue;

        public bool GpuAvailable =>
            GpuTemperature.HasValue;

        public bool Initialize()
        {
            Debug.WriteLine(
                "TemperatureService Initialize");

            try
            {
                TemperatureResult result =
                    RequestTemperature();

                CpuTemperature =
                    result.CpuTemperature;

                GpuTemperature =
                    result.GpuTemperature;

                Debug.WriteLine(
                    $"Provider = {ProviderName}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Temperature service connection failed: {ex}");

                CpuTemperature = null;
                GpuTemperature = null;

                return false;
            }
        }

        public void Update()
        {
            try
            {
                TemperatureResult result =
                    RequestTemperature();

                CpuTemperature =
                    result.CpuTemperature;

                GpuTemperature =
                    result.GpuTemperature;

                LogService.Write(
                    result.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Temperature update failed: {ex}");

                CpuTemperature = null;
                GpuTemperature = null;
            }
        }

        public TemperatureResult GetTemperature()
        {
            return new TemperatureResult
            {
                CpuTemperature =
                    CpuTemperature,

                GpuTemperature =
                    GpuTemperature
            };
        }

        private static TemperatureResult RequestTemperature()
        {
            using NamedPipeClientStream pipe =
                new NamedPipeClientStream(
                    ".",
                    PipeName,
                    PipeDirection.InOut);

            pipe.Connect(1000);

            using StreamReader reader =
                new StreamReader(pipe);

            using StreamWriter writer =
                new StreamWriter(pipe)
                {
                    AutoFlush = true
                };

            writer.WriteLine("GET");

            string? response =
                reader.ReadLine();

            if (string.IsNullOrWhiteSpace(response))
            {
                throw new InvalidOperationException(
                    "Sensor service returned an empty response.");
            }

            return ParseResponse(response);
        }

        private static TemperatureResult ParseResponse(
            string response)
        {
            float? cpu = null;
            float? gpu = null;

            string[] values =
                response.Split(
                    ';',
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string value in values)
            {
                string[] pair =
                    value.Split(
                        '=',
                        2,
                        StringSplitOptions.None);

                if (pair.Length != 2)
                    continue;

                string name =
                    pair[0].Trim();

                string text =
                    pair[1].Trim();

                if (text == "--")
                    continue;

                if (!float.TryParse(
                    text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float temperature))
                {
                    continue;
                }

                if (name.Equals(
                    "CPU",
                    StringComparison.OrdinalIgnoreCase))
                {
                    cpu = temperature;
                }
                else if (name.Equals(
                    "GPU",
                    StringComparison.OrdinalIgnoreCase))
                {
                    gpu = temperature;
                }
            }

            return new TemperatureResult
            {
                CpuTemperature = cpu,
                GpuTemperature = gpu
            };
        }

        public void Shutdown()
        {
        }

        public void Dispose()
        {
        }
    }
}
