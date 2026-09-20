using System.Diagnostics;
using System.IO.Pipes;
using System.ServiceProcess;

namespace AfterIdlerSensorService;

public sealed class SensorService : ServiceBase
{
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;
    private Task? _pipeTask;

    private readonly LhmTemperatureReader _reader;

    public SensorService()
    {
        ServiceName = "AfterIdlerSensorService";

        CanStop = true;
        CanShutdown = true;
        CanPauseAndContinue = false;

        _reader = new LhmTemperatureReader();
    }

    protected override void OnStart(string[] args)
    {
        EventLog.WriteEntry(
            ServiceName,
            "AfterIdler Sensor Service starting.",
            EventLogEntryType.Information);

        _cts = new CancellationTokenSource();

        _reader.Initialize();

        _monitorTask = Task.Run(
            () => MonitorLoopAsync(_cts.Token));

        _pipeTask = Task.Run(
            () => PipeLoopAsync(_cts.Token));
    }

    protected override void OnStop()
    {
        EventLog.WriteEntry(
            ServiceName,
            "AfterIdler Sensor Service stopping.",
            EventLogEntryType.Information);

        _cts?.Cancel();

        try
        {
            _monitorTask?.Wait(TimeSpan.FromSeconds(5));
            _pipeTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch
        {
        }

        _reader.Dispose();

        _cts?.Dispose();
        _cts = null;
    }

    protected override void OnShutdown()
    {
        OnStop();
        base.OnShutdown();
    }

    private async Task MonitorLoopAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _reader.Update();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(
                    ServiceName,
                    $"Sensor update failed: {ex}",
                    EventLogEntryType.Error);
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PipeLoopAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using NamedPipeServerStream pipe =
                    CreatePipe();

                await pipe.WaitForConnectionAsync(
                    cancellationToken);

                using StreamReader reader =
                    new StreamReader(pipe);

                using StreamWriter writer =
                    new StreamWriter(pipe)
                    {
                        AutoFlush = true
                    };

                string? command =
                    await reader.ReadLineAsync();

                if (command == "GET")
                {
                    writer.WriteLine(
                        _reader.GetResponse());
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(
                    ServiceName,
                    $"Pipe error: {ex}",
                    EventLogEntryType.Error);
            }
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        return NamedPipeHelper.CreateServer(
            "AfterIdlerSensor");
    }
}