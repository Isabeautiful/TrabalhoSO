namespace DeadlockDetector.Domain;

public class ResourceAllocationGraph
{
    private readonly object _graphLock = new object();
    private readonly HashSet<string> _processes = new();
    private readonly HashSet<string> _resources = new();
    private readonly List<(string process, string resource)> _requestEdges = new();
    private readonly List<(string resource, string process)> _allocationEdges = new();

    public void AddProcess(string processId)
    {
        lock (_graphLock) _processes.Add(processId);
    }
    
    public void AddResource(string resourceId)
    {
        lock (_graphLock) _resources.Add(resourceId);
    }

    public void RequestResource(string processId, string resourceId)
    {
        lock (_graphLock)
        {
            if (!_processes.Contains(processId)) AddProcess(processId);
            if (!_resources.Contains(resourceId)) AddResource(resourceId);
            _requestEdges.Add((processId, resourceId));
        }
    }

    public void AllocateResource(string resourceId, string processId)
    {
        lock (_graphLock)
        {
            _allocationEdges.Add((resourceId, processId));
            _requestEdges.RemoveAll(r => r.process == processId && r.resource == resourceId);
        }
    }

    public void ReleaseResource(string resourceId, string processId)
    {
        lock (_graphLock)
        {
            _allocationEdges.RemoveAll(e => e.resource == resourceId && e.process == processId);
        }
    }

    public DeadlockResult DetectDeadlock()
    {
        lock (_graphLock)
        {
            var waitForGraph = BuildWaitForGraph();
            var cycles = FindCycles(waitForGraph);
            
            if (cycles.Any())
            {
                var deadlocked = cycles.SelectMany(c => c).Distinct().ToList();
                return new DeadlockResult(true, deadlocked, cycles);
            }
            return new DeadlockResult(false, new List<string>(), new List<List<string>>());
        }
    }

    private Dictionary<string, List<string>> BuildWaitForGraph()
    {
        var waitFor = new Dictionary<string, List<string>>();
        
        foreach (var process in _processes)
        {
            waitFor[process] = new List<string>();
            var requestedResources = _requestEdges.Where(r => r.process == process).Select(r => r.resource);
            
            foreach (var resource in requestedResources)
            {
                var holder = _allocationEdges.FirstOrDefault(a => a.resource == resource).process;
                if (holder != null && holder != process)
                    waitFor[process].Add(holder);
            }
        }
        return waitFor;
    }

    private List<List<string>> FindCycles(Dictionary<string, List<string>> graph)
    {
        var cycles = new List<List<string>>();
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        var path = new List<string>();

        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
                DFS(node, graph, visited, recursionStack, path, cycles);
        }
        return cycles;
    }

    private void DFS(string node, Dictionary<string, List<string>> graph, 
                     HashSet<string> visited, HashSet<string> recursionStack,
                     List<string> path, List<List<string>> cycles)
    {
        visited.Add(node);
        recursionStack.Add(node);
        path.Add(node);

        if (graph.ContainsKey(node))
        {
            foreach (var neighbor in graph[node])
            {
                if (!visited.Contains(neighbor))
                    DFS(neighbor, graph, visited, recursionStack, path, cycles);
                else if (recursionStack.Contains(neighbor))
                {
                    var cycle = new List<string>();
                    int startIndex = path.IndexOf(neighbor);
                    for (int i = startIndex; i < path.Count; i++)
                        cycle.Add(path[i]);
                    cycles.Add(cycle);
                }
            }
        }

        recursionStack.Remove(node);
        path.RemoveAt(path.Count - 1);
    }

    public List<string> GetProcesses() 
    {
        lock (_graphLock) return _processes.ToList();
    }
    
    public List<string> GetResources() 
    {
        lock (_graphLock) return _resources.ToList();
    }
    
    public List<(string resource, string process)> GetAllocationEdges() 
    {
        lock (_graphLock) return _allocationEdges.ToList();
    }
    
    public List<(string process, string resource)> GetRequestEdges() 
    {
        lock (_graphLock) return _requestEdges.ToList();
    }
}