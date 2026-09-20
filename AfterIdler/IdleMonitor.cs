using AfterIdler.Hardware;
using AfterIdler.Models;
using AfterIdler.Services;

namespace AfterIdler.Logic
{

    public sealed class IdleMonitor
    {
        private readonly AppConfig _config;

        public bool IsCountdownRunning { get; private set; }

    
        public int RemainingSeconds { get; private set; }

        public bool ShouldExecuteEndMode { get; private set; }

        private bool _manualCanceled;

        public IdleMonitor(AppConfig config)
        {
            _config = config;
        }

        public void Update(TemperatureResult temperature)
        {
            bool cool = IsCoolEnough(temperature);

            LogService.Write(
                $"Cool={cool} " +
                $"Running={IsCountdownRunning} " +
                $"Manual={_manualCanceled} " +
                $"Remain={RemainingSeconds}");

            ShouldExecuteEndMode = false;

            if (_manualCanceled)
            {
                if (!cool)
                    _manualCanceled = false;

                return;
            }

            if (!cool)
            {
                CancelCountdown();
                return;
            }

            if (!IsCountdownRunning)
            {
                StartCountdown();
                return;
            }

            Tick();
        }

        public bool IsCoolEnough(TemperatureResult temperature)
        {
            bool cpuOk =
                !temperature.CpuAvailable ||
                temperature.CpuTemperature <= _config.TargetCpuTemp;

            bool gpuOk =
                !temperature.GpuAvailable ||
                temperature.GpuTemperature <= _config.TargetGpuTemp;

            LogService.Write(
                $"CPU={temperature.CpuTemperature} " +
                $"GPU={temperature.GpuTemperature} " +
                $"CPU_OK={cpuOk} GPU_OK={gpuOk}");

            return cpuOk && gpuOk;
        }

        public void StartCountdown()
        {
            _manualCanceled = false;

            IsCountdownRunning = true;

            RemainingSeconds = _config.TargetMinutes * 60;

            if (RemainingSeconds <= 0)
                RemainingSeconds = 60;

            LogService.Write("Countdown Start");
        }

        public void CancelCountdown()
        {
            IsCountdownRunning = false;
            RemainingSeconds = 0;

            LogService.Write("Countdown Cancel");
        }

        public void ManualCancel()
        {
            CancelCountdown();
            _manualCanceled = true;
        }

        private void Tick()
        {
            if (RemainingSeconds > 0)
            {
                RemainingSeconds--;
            }

            if (RemainingSeconds <= 0)
            {
                LogService.Write("Countdown Finished");

                RemainingSeconds = 0;
                IsCountdownRunning = false;
                ShouldExecuteEndMode = true;
            }
        }

        public void Reset()
        {
            IsCountdownRunning = false;
            RemainingSeconds = 0;
            ShouldExecuteEndMode = false;
            _manualCanceled = false;

            LogService.Write("Countdown Reset");
        }

        public void Resume()
        {
            _manualCanceled = false;
            IsCountdownRunning = false;
            RemainingSeconds = 0;
            ShouldExecuteEndMode = false;

            LogService.Write("Monitoring Resume");
        }
    }
}