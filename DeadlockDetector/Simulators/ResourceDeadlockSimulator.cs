using System.Collections.Concurrent;
using DeadlockDetector.Domain;

namespace DeadlockDetector.Simulators;

public class ResourceDeadlockSimulator
{
    private readonly ResourceAllocationGraph _graph = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _resources = new();
    private readonly Random _random = new();
    private CancellationTokenSource? _cts;
    private int _deadlockCount = 0;
    private bool _isRunning = false;
    private SimulationConfig _config = new();

    public event Action<string>? OnLog;
    public event Action? OnGraphChanged;

    public ResourceDeadlockSimulator()
    {
        var recursos = new[] { "Impressora", "Scanner", "Arquivo_A", "Arquivo_B", "CD-ROM" };
        foreach (var recurso in recursos)
        {
            _resources[recurso] = new SemaphoreSlim(1, 1);
            _graph.AddResource(recurso);
        }
    }

    public void SetSpeed(string speed)
    {
        _config = SimulationConfig.GetSpeedPreset(speed);
        OnLog?.Invoke($"Velocidade configurada para: {speed}");
    }
    
    public void SetDeadlockChance(string chance)
    {
        _config.DeadlockProbability = SimulationConfig.GetDeadlockProbability(chance);
        OnLog?.Invoke($"Chance de deadlock configurada para: {chance} ({_config.DeadlockProbability * 100}%)");
    }

    public async Task Start(int durationSeconds = 60)
    {
        if (_isRunning) return;
        
        _cts = new CancellationTokenSource();
        _isRunning = true;
        
        var processos = new[] { "P1", "P2", "P3", "P4", "P5" };
        
        for (int i = 0; i < processos.Length; i++)
        {
            var processo = processos[i];
            _graph.AddProcess(processo);
            _ = Task.Run(() => ProcessWork(processo));
            if (i < processos.Length - 1)
                await Task.Delay(_config.ProcessStartDelayMs);
        }
        
        _ = Task.Run(async () =>
        {
            while (_isRunning)
            {
                await Task.Delay(_config.CheckIntervalMs);
                CheckDeadlock();
                OnGraphChanged?.Invoke();
            }
        });
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(durationSeconds * 1000);
            Stop();
        });
    }

    public void Stop()
    {
        _isRunning = false;
        _cts?.Cancel();
        OnLog?.Invoke("Simulacao parada");
    }

    private (string, string) SelectResources(string processId, List<string> resourceList)
    {
        bool tryDeadlock = _random.NextDouble() < _config.DeadlockProbability;
        
        if (tryDeadlock)
        {
            return processId switch
            {
                "P1" => ("Impressora", "Scanner"),
                "P2" => ("Scanner", "Impressora"),
                "P3" => ("Arquivo_A", "Arquivo_B"),
                "P4" => ("Arquivo_B", "Arquivo_A"),
                _ => GetRandomResources(resourceList, false)
            };
        }
        
        return GetRandomResources(resourceList, true);
    }

    private (string, string) GetRandomResources(List<string> resourceList, bool sortAscending)
    {
        int idx1 = _random.Next(resourceList.Count);
        int idx2 = _random.Next(resourceList.Count);
        while (idx2 == idx1) idx2 = _random.Next(resourceList.Count);
        
        if (sortAscending)
        {
            var indices = new List<int> { idx1, idx2 };
            indices.Sort();
            return (resourceList[indices[0]], resourceList[indices[1]]);
        }
        
        return (resourceList[idx1], resourceList[idx2]);
    }

    private async Task ProcessWork(string processId)
    {
        var resourceList = _resources.Keys.ToList();
        
        while (_isRunning && !_cts!.IsCancellationRequested)
        {
            var (resource1, resource2) = SelectResources(processId, resourceList);
            bool tryDeadlock = _random.NextDouble() < _config.DeadlockProbability;
            
            await Task.Delay(_random.Next(10, 50));
            
            _graph.RequestResource(processId, resource1);
            OnGraphChanged?.Invoke();
            
            if (await _resources[resource1].WaitAsync(_config.AcquireTimeoutMs))
            {
                _graph.AllocateResource(resource1, processId);
                OnGraphChanged?.Invoke();
                
                if (tryDeadlock && _random.NextDouble() < 0.3)
                    OnLog?.Invoke($"{processId} adquiriu {resource1} (modo conflito)");
                
                await Task.Delay(_random.Next(20, 100));
                
                _graph.RequestResource(processId, resource2);
                OnGraphChanged?.Invoke();
                
                if (await _resources[resource2].WaitAsync(_config.AcquireTimeoutMs))
                {
                    _graph.AllocateResource(resource2, processId);
                    OnGraphChanged?.Invoke();
                    
                    await Task.Delay(_config.ResourceHoldTimeMs);
                    
                    _resources[resource2].Release();
                    _graph.ReleaseResource(resource2, processId);
                    OnGraphChanged?.Invoke();
                    
                    _resources[resource1].Release();
                    _graph.ReleaseResource(resource1, processId);
                    OnGraphChanged?.Invoke();
                    
                    if (_random.NextDouble() < 0.1)
                        OnLog?.Invoke($"{processId} completou trabalho com {resource1} e {resource2}");
                }
                else
                {
                    if (_random.NextDouble() < 0.2)
                        OnLog?.Invoke($"{processId} timeout esperando {resource2}, liberando {resource1}");
                    
                    _resources[resource1].Release();
                    _graph.ReleaseResource(resource1, processId);
                    OnGraphChanged?.Invoke();
                }
            }
            else
            {
                if (_random.NextDouble() < 0.1)
                    OnLog?.Invoke($"{processId} timeout esperando {resource1}");
                
                _graph.ReleaseResource(resource1, processId);
                OnGraphChanged?.Invoke();
            }
            
            await Task.Delay(_config.WorkDelayMs);
        }
    }

    private void CheckDeadlock()
    {
        var result = _graph.DetectDeadlock();
        if (result.HasDeadlock)
        {
            Interlocked.Increment(ref _deadlockCount);
            OnLog?.Invoke($"DEADLOCK DETECTADO! Processos: {string.Join(", ", result.DeadlockedProcesses)} (Total: {_deadlockCount})");
            
            if (result.DeadlockedProcesses.Contains("P1") && result.DeadlockedProcesses.Contains("P2"))
                OnLog?.Invoke("  -> Deadlock entre P1 e P2 (Impressora <-> Scanner)");
            else if (result.DeadlockedProcesses.Contains("P3") && result.DeadlockedProcesses.Contains("P4"))
                OnLog?.Invoke("  -> Deadlock entre P3 e P4 (Arquivo_A <-> Arquivo_B)");
        }
    }

    public ResourceAllocationGraph GetGraph() => _graph;
    public int GetDeadlockCount() => _deadlockCount;
    public bool IsRunning() => _isRunning;
}