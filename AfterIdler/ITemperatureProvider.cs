using System;

namespace AfterIdler.Hardware
{

    public interface ITemperatureProvider : IDisposable
    {
    
        string Name { get; }
            
        bool Initialize();
            
        TemperatureResult Update();

        void Shutdown();
    }
}