namespace DeadlockDetector.Simulators;

public class SimulationConfig
{
    public int AcquireTimeoutMs { get; set; } = 1000;
    public int WorkDelayMs { get; set; } = 300;
    public int CheckIntervalMs { get; set; } = 500;
    public int ResourceHoldTimeMs { get; set; } = 500;
    public int ProcessStartDelayMs { get; set; } = 100;
    public double DeadlockProbability { get; set; } = 0.4;
    
    public static SimulationConfig GetSpeedPreset(string speed)
    {
        return speed.ToLower() switch
        {
            "muito_lento" => new SimulationConfig 
            { 
                WorkDelayMs = 2000, AcquireTimeoutMs = 3000, 
                ResourceHoldTimeMs = 1500, CheckIntervalMs = 1000 
            },
            "lento" => new SimulationConfig 
            { 
                WorkDelayMs = 1000, AcquireTimeoutMs = 1500, 
                ResourceHoldTimeMs = 800, CheckIntervalMs = 800 
            },
            "normal" => new SimulationConfig 
            { 
                WorkDelayMs = 500, AcquireTimeoutMs = 1000, 
                ResourceHoldTimeMs = 500, CheckIntervalMs = 500 
            },
            "rapido" => new SimulationConfig 
            { 
                WorkDelayMs = 200, AcquireTimeoutMs = 500, 
                ResourceHoldTimeMs = 200, CheckIntervalMs = 300 
            },
            "muito_rapido" => new SimulationConfig 
            { 
                WorkDelayMs = 100, AcquireTimeoutMs = 300, 
                ResourceHoldTimeMs = 100, CheckIntervalMs = 200 
            },
            _ => new SimulationConfig()
        };
    }
    
    public static double GetDeadlockProbability(string chance)
    {
        return chance.ToLower() switch
        {
            "baixa" => 0.15,
            "media" => 0.40,
            "alta" => 0.70,
            "muito_alta" => 0.90,
            _ => 0.40
        };
    }
}