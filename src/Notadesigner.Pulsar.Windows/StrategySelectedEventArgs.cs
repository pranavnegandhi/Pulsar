namespace Notadesigner.Pulsar.Windows;

/// <summary>
/// EventArgs for the StrategySelected event, carrying the Type of the selected strategy.
/// </summary>
/// <remarks>
/// Initializes a new instance of the StrategySelectedEventArgs class.
/// </remarks>
/// <param name="selectedStrategyType">The Type of the selected movement strategy.</param>
public class StrategySelectedEventArgs(Type selectedStrategyType) : EventArgs
{
    /// <summary>
    /// Gets the Type of the selected movement strategy.
    /// </summary>
    public Type SelectedStrategyType { get; } = selectedStrategyType ?? throw new ArgumentNullException(nameof(selectedStrategyType));
}