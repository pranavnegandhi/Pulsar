namespace Notadesigner.Pulsar.Windows;

public class MovementMenu
{
    public ToolStripMenuItem MenuItem { get; }

    public MovementMenu()
    {
        MenuItem = new ToolStripMenuItem("Mo&vement");

        var phantomMenuItem = new ToolStripMenuItem("&Phantom") { Checked = true };
        phantomMenuItem.Click += MenuItemClickHandler;

        var jiggleMenuItem = new ToolStripMenuItem("&Jiggle");
        jiggleMenuItem.Click += MenuItemClickHandler;

        var bezierMenuItem = new ToolStripMenuItem("Be&zier");
        bezierMenuItem.Click += MenuItemClickHandler;

        MenuItem.DropDownItems.Add(phantomMenuItem);
        MenuItem.DropDownItems.Add(jiggleMenuItem);
        MenuItem.DropDownItems.Add(bezierMenuItem);
    }

    private void MenuItemClickHandler(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem clickedItem)
        {
            return;
        }

        // If already checked, ignore the click (cannot uncheck)
        if (clickedItem.Checked)
        {
            return;
        }

        // Uncheck all sibling items, then check the clicked one
        foreach (ToolStripMenuItem item in MenuItem.DropDownItems)
        {
            item.Checked = false;
        }

        clickedItem.Checked = true;
    }
}
