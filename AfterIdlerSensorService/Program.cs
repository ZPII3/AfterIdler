using System.ServiceProcess;

namespace AfterIdlerSensorService;

internal static class Program
{
    static void Main()
    {
        ServiceBase.Run(
            new SensorService());
    }
}