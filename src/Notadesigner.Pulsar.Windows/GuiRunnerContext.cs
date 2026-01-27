using Notadesigner.Pulsar.Windows.Movement;
using Notadesigner.Pulsar.Windows.Properties;

namespace Notadesigner.Pulsar.Windows;

public class GuiRunnerContext : ApplicationContext
{
    private readonly ContextMenuStrip _contextMenu = new();
    private readonly Pulsar _pulsar = new();
    private readonly IconAnimator _iconAnimator;
    private readonly MouseMover _mouseMover = new();

    private readonly MovementMenu _movementMenu; // Hold reference to MovementMenu
    private IPathGenerator _pathGenerator;
    private Type _selectedStrategyType; // Store the current strategy Type

    // Strategy Registry remains the same
    private static readonly Dictionary<Type, Func<IPathGenerator>> _strategyRegistry = new()
    {
        { typeof(PhantomPathGenerator), () => new PhantomPathGenerator() },
        { typeof(JigglePathGenerator), () => new JigglePathGenerator() },
        { typeof(BezierPathGenerator), () => new BezierPathGenerator() }
    };

    // Helper method to get a strategy instance by Type (remains the same)
    private IPathGenerator GetPathGeneratorByType(Type strategyType)
    {
        if (_strategyRegistry.TryGetValue(strategyType, out var createStrategyDelegate))
        {
            return createStrategyDelegate();
        }
        Console.WriteLine($"Warning: Strategy Type '{strategyType.FullName}' not registered. Using default 'JigglePathGenerator'.");
        _selectedStrategyType = typeof(JigglePathGenerator); // Ensure setting also reflects default
        return new JigglePathGenerator();
    }

    public GuiRunnerContext()
    {
        ThreadExit += GuiRunnerContextThreadExitHandler;
        _pulsar.Pulse += PulsarPulseHandler;

        // Load the saved setting string and convert it back to a Type
        string savedStrategyFullName = Preferences.Default.SelectedMovementStrategy;
        if (!string.IsNullOrEmpty(savedStrategyFullName))
        {
            // Attempt to get the Type from its full name.
            // Ensure the application assembly is correctly referenced or use Assembly.GetExecutingAssembly().GetType() if needed.
            _selectedStrategyType = Type.GetType(savedStrategyFullName) ?? typeof(JigglePathGenerator);
        }
        else
        {
            _selectedStrategyType = typeof(JigglePathGenerator); // Default if no setting
        }

        _pathGenerator = GetPathGeneratorByType(_selectedStrategyType); // Instantiate using the resolved Type

        var icon = new NotifyIcon()
        {
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        _iconAnimator = new IconAnimator(icon);
        icon.Click += IconClickHandler;

        // --- Menu Setup ---
        // Initialize MovementMenu (no longer needs context in constructor)
        _movementMenu = new MovementMenu();
        // Subscribe to the StrategySelected event
        _movementMenu.StrategySelected += MenuStrategySelectedHandler; // Hook up the event handler

        // Add the Movement menu to the main context menu
        _contextMenu.Items.Add(_movementMenu.MenuItem);

        // Add other menu items (Start, Interrupt, Exit) - assumed to be unchanged
        var startMenuItem = new ToolStripMenuItem("S&tart");
        startMenuItem.Click += async (_, _) => await StartJigglerAsync();
        _contextMenu.Items.Add(startMenuItem);

        var interruptMenuItem = new ToolStripMenuItem("I&nterrupt");
        interruptMenuItem.Click += async (_, _) => await StopJigglerAsync();
        _contextMenu.Items.Add(interruptMenuItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitMenuItem = new ToolStripMenuItem("E&xit");
        exitMenuItem.Click += (_, _) => Application.Exit();
        _contextMenu.Items.Add(exitMenuItem);
        // --- End Menu Setup ---

        // Update the checked state in the menu based on the loaded strategy type
        _movementMenu.UpdateCheckedState(_selectedStrategyType);
    }

    /// <summary>
    /// Handles the StrategySelected event from MovementMenu.
    /// </summary>
    private void MenuStrategySelectedHandler(object? sender, StrategySelectedEventArgs e)
    {
        // Check if the selection is actually different to avoid unnecessary work
        if (_selectedStrategyType == e.SelectedStrategyType)
        {
            return; // No change, do nothing
        }

        // Update the application's current strategy Type
        _selectedStrategyType = e.SelectedStrategyType;
        _pathGenerator = GetPathGeneratorByType(_selectedStrategyType); // Re-instantiate the generator

        // Save the new setting by storing the Type's full name
        Preferences.Default.SelectedMovementStrategy = _selectedStrategyType.FullName;
        Preferences.Default.Save();

        // Update the checked state in the MovementMenu UI
        _movementMenu.UpdateCheckedState(_selectedStrategyType);

        // Optional: If the strategy change needs to take effect immediately on the next pulse,
        // you might want to restart the pulsar or notify it.
        // For now, it will take effect on the next pulse.
    }

    private async Task StartJigglerAsync()
    {
        _iconAnimator.Cancel();
        await _iconAnimator.AnimateStartAsync();
        _pulsar.Start();
    }

    private async Task StopJigglerAsync()
    {
        _iconAnimator.Cancel();
        await _iconAnimator.AnimateStopAsync();
        await _pulsar.StopAsync();
    }

    private async void PulsarPulseHandler(object? sender, EventArgs e)
    {
        try
        {
            var screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            var path = _pathGenerator.GeneratePath(screenBounds.Width, screenBounds.Height);
            await _mouseMover.ExecutePathAsync(path);
        }
        catch (OperationCanceledException)
        {
            // Animation cancelled, that's fine
        }
    }

    private async void GuiRunnerContextThreadExitHandler(object? sender, EventArgs e)
    {
        await _pulsar.DisposeAsync();
    }

    private async void IconClickHandler(object? sender, EventArgs e)
    {
        if (e is not MouseEventArgs mouseArgs || mouseArgs.Button != MouseButtons.Left)
        {
            return;
        }

        if (_pulsar.IsRunning)
        {
            await StopJigglerAsync();
        }
        else
        {
            await StartJigglerAsync();
        }
    }
}