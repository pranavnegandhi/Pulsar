using Notadesigner.Pulsar.Windows.Movement; // For IPathGenerator and concrete types

namespace Notadesigner.Pulsar.Windows;

public class MovementMenu
{
    public ToolStripMenuItem MenuItem { get; } // The root "Movement" menu item

    // Map menu display text to the actual Type object for compile-time safety
    private readonly Dictionary<string, Type> _menuTextToTypeMap = new()
    {
        { "&Phantom", typeof(PhantomPathGenerator) },
        { "&Jiggle", typeof(JigglePathGenerator) },
        { "Be&zier", typeof(BezierPathGenerator) }
        // Add new strategy types here, matching the text used in ToolStripMenuItem constructor
    };

    /// <summary>
    /// Event raised when a movement strategy is selected from the menu.
    /// </summary>
    public event EventHandler<StrategySelectedEventArgs>? StrategySelected;

    // Constructor no longer takes the owner context
    public MovementMenu()
    {
        MenuItem = new ToolStripMenuItem("Mo&vement");

        // Dynamically create menu items from the map
        foreach (var pair in _menuTextToTypeMap)
        {
            var menuItem = new ToolStripMenuItem(pair.Key); // e.g., "&Phantom"
            menuItem.Click += MenuItemClickHandler;
            MenuItem.DropDownItems.Add(menuItem);
        }
    }

    private void MenuItemClickHandler(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem clickedItem)
        {
            return;
        }

        // Look up the Type associated with the clicked menu item's text
        if (_menuTextToTypeMap.TryGetValue(clickedItem?.Text ?? string.Empty, out Type? selectedType))
        {
            // Raise the event, passing the selected Type
            OnStrategySelected(selectedType);
        }
        else
        {
            // This case should ideally not happen if the map is kept in sync
            Console.WriteLine($"Error: Could not find Type for menu item text '{clickedItem!.Text}'.");
        }
    }

    /// <summary>
    /// Raises the StrategySelected event.
    /// </summary>
    /// <param name="selectedStrategyType">The Type of the selected strategy.</param>
    protected virtual void OnStrategySelected(Type selectedStrategyType)
    {
        StrategySelected?.Invoke(this, new StrategySelectedEventArgs(selectedStrategyType));
    }

    /// <summary>
    /// Updates the checked state of menu items to reflect the current strategy.
    /// </summary>
    /// <param name="currentStrategyType">The Type of the currently active strategy.</param>
    public void UpdateCheckedState(Type currentStrategyType)
    {
        foreach (ToolStripMenuItem item in MenuItem.DropDownItems)
        {
            if (item is null || item.Text is null)
            {
                continue;
            }

            // Check if this item's associated Type matches the current strategy Type
            if (_menuTextToTypeMap.TryGetValue(item.Text, out Type? itemType) && itemType == currentStrategyType)
            {
                item.Checked = true;
            }
            else
            {
                item.Checked = false;
            }
        }
    }
}