using Cairo;
using DeadlockDetector.Domain;
using Color = Cairo.Color;

namespace DeadlockDetector.UI;

public class GraphDrawer
{
    private readonly Context _cr;
    private readonly double _width;
    private readonly double _height;
    private List<GraphNode> _nodes = new();

    public GraphDrawer(Context cr, double width, double height)
    {
        _cr = cr;
        _width = width;
        _height = height;
    }

    public void Draw(ResourceAllocationGraph graph, List<string> deadlockedProcesses)
    {
        DrawBackground();
        BuildNodes(graph, deadlockedProcesses);
        
        if (_nodes.Count == 0) return;
        
        CalculateLayout();
        DrawEdges(graph);
        DrawNodes();
        DrawLegend();
    }

    private void DrawBackground()
    {
        _cr.SetSourceRGB(GraphColors.BackgroundColor.R, GraphColors.BackgroundColor.G, GraphColors.BackgroundColor.B);
        _cr.Rectangle(0, 0, _width, _height);
        _cr.Fill();
    }

    private void BuildNodes(ResourceAllocationGraph graph, List<string> deadlockedProcesses)
    {
        _nodes.Clear();
        
        foreach (var process in graph.GetProcesses())
        {
            _nodes.Add(new GraphNode
            {
                Id = process,
                Label = process,
                Type = "process",
                IsDeadlocked = deadlockedProcesses?.Contains(process) ?? false
            });
        }
        
        foreach (var resource in graph.GetResources())
        {
            _nodes.Add(new GraphNode
            {
                Id = resource,
                Label = resource,
                Type = "resource",
                IsDeadlocked = false
            });
        }
    }

    private void CalculateLayout()
    {
        float centerX = (float)_width / 2;
        float centerY = (float)_height / 2;
        float radius = (float)Math.Min(_width, _height) / 2.5f;
        
        for (int i = 0; i < _nodes.Count; i++)
        {
            double angle = (2 * Math.PI * i) / _nodes.Count;
            _nodes[i].X = centerX + radius * (float)Math.Cos(angle);
            _nodes[i].Y = centerY + radius * (float)Math.Sin(angle);
        }
    }

    private void DrawEdges(ResourceAllocationGraph graph)
    {
        DrawEdgeSet(graph.GetAllocationEdges(), GraphColors.AllocationColor, true);
        DrawEdgeSet(graph.GetRequestEdges(), GraphColors.RequestColor, false);
    }

    private void DrawEdgeSet<T>(List<T> edges, Color color, bool isAllocation) where T : struct
    {
        _cr.SetSourceRGB(color.R, color.G, color.B);
        _cr.LineWidth = 2;
        
        foreach (var edge in edges)
        {
            GraphNode? source = null;
            GraphNode? target = null;
            
            if (isAllocation)
            {
                var alloc = (ValueTuple<string, string>)Convert.ChangeType(edge, typeof((string, string)));
                source = _nodes.Find(n => n.Id == alloc.Item1);
                target = _nodes.Find(n => n.Id == alloc.Item2);
            }
            else
            {
                var req = (ValueTuple<string, string>)Convert.ChangeType(edge, typeof((string, string)));
                source = _nodes.Find(n => n.Id == req.Item1);
                target = _nodes.Find(n => n.Id == req.Item2);
            }
            
            if (source != null && target != null)
            {
                _cr.MoveTo(source.X, source.Y);
                _cr.LineTo(target.X, target.Y);
                _cr.Stroke();
                DrawArrow(source.X, source.Y, target.X, target.Y, color);
            }
        }
    }

    private void DrawArrow(double x1, double y1, double x2, double y2, Color color)
    {
        double angle = Math.Atan2(y2 - y1, x2 - x1);
        double arrowSize = 10;
        double arrowX = x2 - 15 * Math.Cos(angle);
        double arrowY = y2 - 15 * Math.Sin(angle);
        
        _cr.SetSourceRGB(color.R, color.G, color.B);
        _cr.MoveTo(arrowX, arrowY);
        _cr.LineTo(arrowX - arrowSize * Math.Cos(angle - Math.PI / 6), 
                  arrowY - arrowSize * Math.Sin(angle - Math.PI / 6));
        _cr.LineTo(arrowX - arrowSize * Math.Cos(angle + Math.PI / 6), 
                  arrowY - arrowSize * Math.Sin(angle + Math.PI / 6));
        _cr.ClosePath();
        _cr.Fill();
    }

    private void DrawNodes()
    {
        foreach (var node in _nodes)
        {
            Color color;
            if (node.IsDeadlocked)
                color = GraphColors.DeadlockColor;
            else if (node.Type == "process")
                color = GraphColors.ProcessColor;
            else
                color = GraphColors.ResourceColor;
            
            _cr.SetSourceRGB(color.R, color.G, color.B);
            _cr.LineWidth = 2;
            
            double size = node.Type == "process" ? 30 : 25;
            
            if (node.Type == "process")
            {
                _cr.Arc(node.X, node.Y, size / 2, 0, 2 * Math.PI);
                _cr.FillPreserve();
                _cr.SetSourceRGB(1, 1, 1);
                _cr.Stroke();
                
                _cr.SetSourceRGB(GraphColors.TextColor.R, GraphColors.TextColor.G, GraphColors.TextColor.B);
                _cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Bold);
                _cr.SetFontSize(14);
                var text = node.Label.Length > 0 ? node.Label[0].ToString() : "P";
                var extents = _cr.TextExtents(text);
                _cr.MoveTo(node.X - extents.Width / 2, node.Y + extents.Height / 2);
                _cr.ShowText(text);
            }
            else
            {
                _cr.Rectangle(node.X - size / 2, node.Y - size / 2.5, size, size / 1.5);
                _cr.FillPreserve();
                _cr.SetSourceRGB(1, 1, 1);
                _cr.Stroke();
                
                _cr.SetSourceRGB(0.9, 0.9, 0.9);
                _cr.Arc(node.X, node.Y - 2, 3, 0, 2 * Math.PI);
                _cr.Fill();
            }
            
            _cr.SetSourceRGB(GraphColors.TextColor.R, GraphColors.TextColor.G, GraphColors.TextColor.B);
            _cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
            _cr.SetFontSize(9);
            var labelExtents = _cr.TextExtents(node.Label);
            double labelY = node.Type == "process" ? node.Y + size / 2 + 8 : node.Y + size / 2 + 5;
            _cr.MoveTo(node.X - labelExtents.Width / 2, labelY);
            _cr.ShowText(node.Label);
        }
    }

    private void DrawLegend()
    {
        double x = 10;
        double y = _height - 100;
        double spacing = 85;
        
        _cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
        _cr.SetFontSize(9);
        
        // Processo
        _cr.SetSourceRGB(GraphColors.ProcessColor.R, GraphColors.ProcessColor.G, GraphColors.ProcessColor.B);
        _cr.Arc(x + 6, y + 6, 6, 0, 2 * Math.PI);
        _cr.Fill();
        _cr.SetSourceRGB(GraphColors.TextColor.R, GraphColors.TextColor.G, GraphColors.TextColor.B);
        _cr.MoveTo(x + 18, y + 3);
        _cr.ShowText("Processo");
        
        // Recurso
        _cr.SetSourceRGB(GraphColors.ResourceColor.R, GraphColors.ResourceColor.G, GraphColors.ResourceColor.B);
        _cr.Rectangle(x + spacing, y, 12, 12);
        _cr.Fill();
        _cr.SetSourceRGB(GraphColors.TextColor.R, GraphColors.TextColor.G, GraphColors.TextColor.B);
        _cr.MoveTo(x + spacing + 18, y + 3);
        _cr.ShowText("Recurso");
        
        // Deadlock
        _cr.SetSourceRGB(GraphColors.DeadlockColor.R, GraphColors.DeadlockColor.G, GraphColors.DeadlockColor.B);
        _cr.Arc(x + spacing * 2 + 6, y + 6, 6, 0, 2 * Math.PI);
        _cr.Fill();
        _cr.SetSourceRGB(GraphColors.TextColor.R, GraphColors.TextColor.G, GraphColors.TextColor.B);
        _cr.MoveTo(x + spacing * 2 + 18, y + 3);
        _cr.ShowText("Deadlock");
        
        // Solicitacao
        _cr.SetSourceRGB(GraphColors.RequestColor.R, GraphColors.RequestColor.G, GraphColors.RequestColor.B);
        _cr.LineWidth = 2;
        _cr.MoveTo(x + spacing * 3, y + 6);
        _cr.LineTo(x + spacing * 3 + 20, y + 6);
        _cr.Stroke();
        _cr.SetSourceRGB(GraphColors.TextColor.R, GraphColors.TextColor.G, GraphColors.TextColor.B);
        _cr.MoveTo(x + spacing * 3 + 28, y + 3);
        _cr.ShowText("Solicitacao");
        
        // Alocacao
        _cr.SetSourceRGB(GraphColors.AllocationColor.R, GraphColors.AllocationColor.G, GraphColors.AllocationColor.B);
        _cr.MoveTo(x + spacing * 4 + 20, y + 6);
        _cr.LineTo(x + spacing * 4 + 40, y + 6);
        _cr.Stroke();
        _cr.SetSourceRGB(GraphColors.TextColor.R, GraphColors.TextColor.G, GraphColors.TextColor.B);
        _cr.MoveTo(x + spacing * 4 + 48, y + 3);
        _cr.ShowText("Alocacao");
    }
}