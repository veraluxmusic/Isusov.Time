using Isusov.Time.Core;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Event payload raised when the simulation speed changes.
  /// </summary>
  public readonly struct SpeedChangedEvent
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SpeedChangedEvent"/> struct.
    /// </summary>
    /// <param name="previousSpeed">The speed before the change.</param>
    /// <param name="currentSpeed">The speed after the change.</param>
    public SpeedChangedEvent(SimulationSpeed previousSpeed, SimulationSpeed currentSpeed)
    {
      PreviousSpeed = previousSpeed;
      CurrentSpeed = currentSpeed;
    }

    /// <summary>
    /// Gets the simulation speed before the change.
    /// </summary>
    public SimulationSpeed PreviousSpeed { get; }

    /// <summary>
    /// Gets the simulation speed after the change.
    /// </summary>
    public SimulationSpeed CurrentSpeed { get; }
  }
}
