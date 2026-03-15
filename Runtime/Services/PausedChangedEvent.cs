namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised when the paused state changes.
  /// </summary>
  public readonly struct PausedChangedEvent
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PausedChangedEvent"/> struct.
    /// </summary>
    /// <param name="previousPaused">The paused state before the change.</param>
    /// <param name="currentPaused">The paused state after the change.</param>
    public PausedChangedEvent(bool previousPaused, bool currentPaused)
    {
      PreviousPaused = previousPaused;
      CurrentPaused = currentPaused;
    }

    /// <summary>
    /// Gets the paused state before the change.
    /// </summary>
    public bool PreviousPaused { get; }

    /// <summary>
    /// Gets the paused state after the change.
    /// </summary>
    public bool CurrentPaused { get; }
  }
}
