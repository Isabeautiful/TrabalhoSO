using Gtk;
using DeadlockDetector.Domain;
using DeadlockDetector.Simulators;

namespace DeadlockDetector.UI;

public class MainWindow : Gtk.Window
{
    private ResourceDeadlockSimulator? _simulator;
    private DrawingArea? _drawingArea;
    private TextView? _logTextView;
    private TextBuffer? _logBuffer;
    private Button? _btnStart;
    private Button? _btnStop;
    private Button? _btnReset;
    private Button? _btnNext;
    private Label? _lblStatus;
    private uint _timeoutId;
    private DateTime _lastGraphUpdate = DateTime.MinValue;
    private readonly TimeSpan _minUpdateInterval = TimeSpan.FromMilliseconds(50);

    // private ComboBoxText? _cmbSpeed;
    private ComboBoxText? _cmbDeadlockChance;
    // private Entry? _txtDuration;

    public MainWindow() : base("Deadlock Detector - Resource Allocation Graph")
    {
        SetDefaultSize(1300, 800);
        SetPosition(WindowPosition.Center);
        DeleteEvent += (o, args) => Application.Quit();

        var vbox = new Box(Orientation.Vertical, 5);
        Add(vbox);

        CreateToolbar(vbox);
        CreateMainArea(vbox);

        ShowAll();
        InitializeSimulator();

        _timeoutId = GLib.Timeout.Add(250, UpdateStatus);
    }

    private void CreateToolbar(Box vbox)
    {
        var toolbar = new Box(Orientation.Horizontal, 5);
        toolbar.Margin = 5;

        _btnStart = new Button("Iniciar");
        _btnStart.Clicked += OnStartClicked;

        _btnStop = new Button("Parar");
        _btnStop.Clicked += OnStopClicked;
        _btnStop.Sensitive = false;

        _btnReset = new Button("Reset");
        _btnReset.Clicked += OnResetClicked;

        _btnNext = new Button("Próximo");
        _btnNext.Clicked += OnNextClicked;
        _btnNext.Sensitive = false;

        var lblSpeed = new Label("Velocidade:");
        // _cmbSpeed = new ComboBoxText();
        // _cmbSpeed.AppendText("Muito Lento");
        // _cmbSpeed.AppendText("Lento");
        // _cmbSpeed.AppendText("Normal");
        // _cmbSpeed.AppendText("Rapido");
        // _cmbSpeed.AppendText("Muito Rapido");
        // _cmbSpeed.Active = 2;

        var lblChance = new Label("Chance Deadlock:");
        _cmbDeadlockChance = new ComboBoxText();
        _cmbDeadlockChance.AppendText("Baixa");
        _cmbDeadlockChance.AppendText("Media");
        _cmbDeadlockChance.AppendText("Alta");
        _cmbDeadlockChance.AppendText("Muito Alta");
        _cmbDeadlockChance.Active = 1;

        // var lblDuration = new Label("Duracao(s):");
        // _txtDuration = new Entry();
        // _txtDuration.Text = "60";
        // _txtDuration.WidthRequest = 50;

        _lblStatus = new Label("Pronto");

        toolbar.PackStart(_btnStart, false, false, 0);
        toolbar.PackStart(_btnStop, false, false, 0);
        toolbar.PackStart(_btnNext, false, false, 0);
        toolbar.PackStart(_btnReset, false, false, 0);
        toolbar.PackStart(new Separator(Orientation.Vertical), false, false, 10);
        toolbar.PackStart(lblSpeed, false, false, 0);
        // toolbar.PackStart(_cmbSpeed, false, false, 0);
        toolbar.PackStart(lblChance, false, false, 0);
        toolbar.PackStart(_cmbDeadlockChance, false, false, 0);
        // toolbar.PackStart(lblDuration, false, false, 0);
        // toolbar.PackStart(_txtDuration, false, false, 0);
        toolbar.PackStart(_lblStatus, false, false, 10);

        vbox.PackStart(toolbar, false, false, 0);
    }

    private void CreateMainArea(Box vbox)
    {
        var paned = new Paned(Orientation.Horizontal);

        _drawingArea = new DrawingArea();
        _drawingArea.SetSizeRequest(850, 700);
        _drawingArea.Drawn += OnDrawGraph;

        var scrolledWindow = new ScrolledWindow();
        scrolledWindow.SetSizeRequest(400, 700);
        _logTextView = new TextView();
        _logTextView.Editable = false;
        _logTextView.WrapMode = WrapMode.Word;
        _logBuffer = _logTextView.Buffer;
        scrolledWindow.Child = _logTextView;

        paned.Pack1(_drawingArea, true, false);
        paned.Pack2(scrolledWindow, false, false);

        vbox.PackStart(paned, true, true, 0);
    }

    private void InitializeSimulator()
    {
        _simulator = new ResourceDeadlockSimulator();
        _simulator.OnLog += AddLogMessage;

        _simulator.OnGraphChanged += () =>
        {
            Application.Invoke(delegate
            {
                _drawingArea?.QueueDraw();
            });
        };

        ApplySettings();
    }

    private void ApplySettings()
    {
        if (_simulator == null) return;

        // if (_cmbSpeed?.ActiveText != null)
        // {
        //     string speed = _cmbSpeed.ActiveText.ToLower().Replace(" ", "_");
        //     _simulator.SetSpeed(speed);
        // }

        if (_cmbDeadlockChance?.ActiveText != null)
        {
            string chance = _cmbDeadlockChance.ActiveText.ToLower().Replace(" ", "_");
            _simulator.SetDeadlockChance(chance);
        }
    }

    private int GetDuration()
    {
        // if (
        //     _txtDuration != null
        //     && int.TryParse(_txtDuration.Text, out int duration)
        // )
        // {
        //     return Math.Clamp(duration, 5, 300);
        // }

        return 60;
    }

    private void OnStartClicked(object? sender, EventArgs e)
    {
        if (_btnStart == null || _btnStop == null) return;

        ApplySettings();
        _simulator?.SetManualMode(true);

        _btnStart.Sensitive = false;
        _btnStop.Sensitive = true;
        if (_btnNext != null) _btnNext.Sensitive = true;

        // if (_cmbSpeed != null) _cmbSpeed.Sensitive = false;
        if (_cmbDeadlockChance != null) _cmbDeadlockChance.Sensitive = false;
        // if (_txtDuration != null) _txtDuration.Sensitive = false;

        AddLogMessage("SIMULACAO INICIADA");
        _ = _simulator?.Start(GetDuration());
    }

    private void OnNextClicked(object? sender, EventArgs e)
    {
        if (_btnNext == null || _simulator == null) return;

        // O botão será reabilitado em OnDrawGraph após a confirmação do redraw.
        _btnNext.Sensitive = false;
        _simulator.ReleaseStep();
    }

    private void OnStopClicked(object? sender, EventArgs e)
    {
        if (_btnStart == null || _btnStop == null) return;

        _simulator?.Stop();

        _btnStart.Sensitive = true;
        _btnStop.Sensitive = false;
        if (_btnNext != null) _btnNext.Sensitive = false;

        // if (_cmbSpeed != null) _cmbSpeed.Sensitive = true;
        if (_cmbDeadlockChance != null) _cmbDeadlockChance.Sensitive = true;
        // if (_txtDuration != null) _txtDuration.Sensitive = true;

        AddLogMessage("SIMULACAO PARADA");
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        if (_btnStart == null || _btnStop == null) return;

        _simulator?.Stop();
        InitializeSimulator();
        _drawingArea?.QueueDraw();

        _btnStart.Sensitive = true;
        _btnStop.Sensitive = false;
        if (_btnNext != null) _btnNext.Sensitive = false;

        // if (_cmbSpeed != null) _cmbSpeed.Sensitive = true;
        if (_cmbDeadlockChance != null) _cmbDeadlockChance.Sensitive = true;
        // if (_txtDuration != null) _txtDuration.Sensitive = true;

        AddLogMessage("SIMULACAO REINICIADA");
    }

    private bool UpdateStatus()
    {
        if (_simulator != null && _lblStatus != null)
        {
            var graph = _simulator.GetGraph();
            var status = $"Processos: {graph.GetProcesses().Count} | ";
            status += $"Recursos: {graph.GetResources().Count} | ";
            status += $"Deadlocks: {_simulator.GetDeadlockCount()} | ";
            status += $"Status: {(_simulator.IsRunning() ? "Executando" : "Parado")}";
            _lblStatus.Text = status;
        }
        return true;
    }

    private void AddLogMessage(string message)
    {
        Application.Invoke(delegate
        {
            if (_logBuffer == null || _logTextView == null) return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _logBuffer.InsertAtCursor($"[{timestamp}] {message}\n");
            var iter = _logBuffer.GetIterAtLine(_logBuffer.LineCount - 1);
            _logTextView.ScrollToIter(iter, 0, false, 0, 0);

            if (_logBuffer.LineCount > 1000)
            {
                var startIter = _logBuffer.GetIterAtLine(0);
                var endIter = _logBuffer.GetIterAtLine(100);
                _logBuffer.Delete(ref startIter, ref endIter);
            }
        });
    }

    private void OnDrawGraph(object? sender, DrawnArgs args)
    {
        if (_simulator == null || _drawingArea == null) return;

        var graph = _simulator.GetGraph();
        var result = graph.DetectDeadlock();

        var drawer = new GraphDrawer(args.Cr, _drawingArea.Allocation.Width, _drawingArea.Allocation.Height);
        drawer.Draw(graph, result.DeadlockedProcesses);

        _simulator.ConfirmDraw();

        if (_simulator.IsRunning() && _btnNext != null)
            _btnNext.Sensitive = true;
    }
}