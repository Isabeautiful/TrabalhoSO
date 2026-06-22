using Cairo;
using DeadlockDetector.Domain;
using Color = Cairo.Color;

namespace DeadlockDetector.UI;

public class GraphDrawer
{
    private readonly Context _cr;
    private readonly double _width;
    private readonly double _height;
    private readonly List<GraphNode> _nodes = [];

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
        _cr.NewPath();
        _cr.SetSourceRGB(color.R, color.G, color.B);
        _cr.LineWidth = 2;

        foreach (var edge in edges)
        {
            var edgeStrs = (ValueTuple<string, string>)Convert.ChangeType(edge, typeof((string, string)));
            GraphNode? source = _nodes.Find(n => n.Id == edgeStrs.Item1);
            GraphNode? target = _nodes.Find(n => n.Id == edgeStrs.Item2);

            if (source != null && target != null)
            {
                _cr.NewPath();
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
        double arrowSize = 15;
        double arrowX = x2 - 12 * Math.Cos(angle);
        double arrowY = y2 - 12 * Math.Sin(angle);

        _cr.NewPath();
        _cr.SetSourceRGB(color.R, color.G, color.B);
        _cr.MoveTo(arrowX, arrowY);

        _cr.LineTo(
            arrowX - arrowSize * Math.Cos(angle - Math.PI / 6),
            arrowY - arrowSize * Math.Sin(angle - Math.PI / 6)
        );

        _cr.LineTo(
            arrowX - arrowSize * Math.Cos(angle + Math.PI / 6),
            arrowY - arrowSize * Math.Sin(angle + Math.PI / 6)
        );

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

            if (node.Type == "process")
                DrawProcessNode(node, color);
            else
                DrawResourceNode(node, color);

            DrawNodeLabel(node);
        }
    }

    private void DrawProcessNode(GraphNode node, Color color)
    {
        const double size = 30;

        _cr.NewPath();
        _cr.SetSourceRGB(color.R, color.G, color.B);
        _cr.LineWidth = 2;

        _cr.Arc(node.X, node.Y, size / 2, 0, 2 * Math.PI);
        _cr.FillPreserve();

        _cr.NewPath();
        _cr.SetSourceRGB(1, 1, 1);
        _cr.Stroke();

        _cr.NewPath();
        _cr.SetSourceRGB(
            GraphColors.NodeLabelColor.R,
            GraphColors.NodeLabelColor.G,
            GraphColors.NodeLabelColor.B
        );

        _cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Bold);
        _cr.SetFontSize(14);

        var text = node.Label.Length > 0
            ? node.Label[0].ToString()
            : "P";

        var extents = _cr.TextExtents(text);

        _cr.MoveTo(
            node.X - extents.Width / 2,
            node.Y + extents.Height / 2
        );

        _cr.ShowText(text);
    }

    private void DrawResourceNode(GraphNode node, Color color)
    {
        const double size = 35;

        _cr.NewPath();
        _cr.SetSourceRGB(color.R, color.G, color.B);
        _cr.LineWidth = 2;

        _cr.Rectangle(
            node.X - size / 2,
            node.Y - size / 2.5,
            size,
            size / 1.5
        );

        _cr.FillPreserve();

        _cr.NewPath();
        _cr.SetSourceRGB(1, 1, 1);
        _cr.Stroke();

        _cr.NewPath();
        _cr.SetSourceRGB(0.9, 0.9, 0.9);

        _cr.Arc(node.X, node.Y - 2, 3, 0, 2 * Math.PI);
        _cr.Fill();
    }

    private void DrawNodeLabel(GraphNode node)
    {
        double size = node.Type == "process" ? 30 : 25;


        _cr.NewPath();
        _cr.SetSourceRGB(
            GraphColors.NodeLabelColor.R,
            GraphColors.NodeLabelColor.G,
            GraphColors.NodeLabelColor.B
        );

        _cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
        _cr.SetFontSize(14);

        var extents = _cr.TextExtents(node.Label);

        double sideFactor = node.Type == "process" ? 1 : -1;
        double labelY = node.Y + sideFactor * (size / 2 + size * 0.5);

        _cr.MoveTo(
            node.X - extents.Width / 2,
            labelY
        );

        _cr.ShowText(node.Label);
    }

    private void DrawLegend()
    {
        const double panelWidth = 160;
        const double rowHeight = 28;

        const double panelX = 10;

        const int labelAmount = 5;
        double panelHeight = labelAmount * rowHeight + 10;
        double panelY = _height - panelHeight - 10;

        const double iconX = panelX + 14;
        const double textX = panelX + 36;


        _cr.NewPath();
        _cr.SetSourceRGB(
            GraphColors.LegendBackgroundColor.R,
            GraphColors.LegendBackgroundColor.G,
            GraphColors.LegendBackgroundColor.B
        );

        _cr.Rectangle(panelX, panelY, panelWidth, panelHeight);
        _cr.FillPreserve();


        _cr.NewPath();
        _cr.SetSourceRGB(0.75, 0.75, 0.75);
        _cr.LineWidth = 1;
        _cr.Stroke();


        _cr.NewPath();
        _cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
        _cr.SetFontSize(16);

        DrawLegendSquare(iconX, panelY + 18 + rowHeight * 0, GraphColors.ResourceColor, textX, "Recurso");
        DrawLegendCircle(iconX, panelY + 18 + rowHeight * 1, GraphColors.ProcessColor, textX, "Processo");
        DrawLegendCircle(iconX, panelY + 18 + rowHeight * 2, GraphColors.DeadlockColor, textX, "Em deadlock");
        DrawLegendLine(iconX, panelY + 18 + rowHeight * 3, GraphColors.RequestColor, textX, "Solicitação");
        DrawLegendLine(iconX, panelY + 18 + rowHeight * 4, GraphColors.AllocationColor, textX, "Alocação");
    }

    private void DrawLegendCircle(double x, double y, Color color, double textX, string text)
    {
        _cr.SetSourceRGB(color.R, color.G, color.B);
        _cr.Arc(x, y, 10, 0, 2 * Math.PI);
        _cr.Fill();

        DrawLegendText(textX, y + 5, text);
    }

    private void DrawLegendSquare(double x, double y, Color color, double textX, string text)
    {
        _cr.SetSourceRGB(color.R, color.G, color.B);
        const double size = 15;

        _cr.Rectangle(
            x - size / 2,
            y - size / 2,
            size,
            size
        );

        _cr.Fill();

        DrawLegendText(textX, y + 5, text);
    }

    private void DrawLegendLine(double x, double y, Color color, double textX, string text)
    {
        _cr.SetSourceRGB(color.R, color.G, color.B);
        _cr.LineWidth = 6;

        _cr.MoveTo(x - 8, y + 2);
        _cr.LineTo(x + 8, y + 2);
        _cr.Stroke();

        DrawLegendText(textX, y + 5, text);
    }

    private void DrawLegendText(double x, double y, string text)
    {
        _cr.SetSourceRGB(
            GraphColors.LegendLabelColor.R,
            GraphColors.LegendLabelColor.G,
            GraphColors.LegendLabelColor.B
        );

        _cr.MoveTo(x, y);
        _cr.ShowText(text);
    }

}