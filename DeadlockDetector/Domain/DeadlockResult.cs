namespace DeadlockDetector.Domain;

public class DeadlockResult
{
    public bool HasDeadlock { get; }
    public List<string> DeadlockedProcesses { get; }
    public List<List<string>> Cycles { get; }

    public DeadlockResult(bool hasDeadlock, List<string> deadlockedProcesses, List<List<string>> cycles)
    {
        HasDeadlock = hasDeadlock;
        DeadlockedProcesses = deadlockedProcesses;
        Cycles = cycles;
    }
}