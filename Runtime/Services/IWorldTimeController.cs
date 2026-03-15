using Isusov.Time.Core;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Mutation and lifecycle control surface for the world-time runtime.
  /// </summary>
  /// <remarks>
  /// Prefer this interface for bootstrap and orchestration code that is responsible for advancing,
  /// pausing, restoring, or otherwise controlling the simulation.
  /// </remarks>
  public interface IWorldTimeController
  {
    /// <summary>
    /// Initializes the service and constructs its runtime dependencies.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Attempts to initialize the service without throwing for expected configuration failures.
    /// </summary>
    /// <param name="errorMessage">
    /// When this method returns <see langword="false"/>, contains the reason initialization failed;
    /// otherwise an empty string.
    /// </param>
    /// <returns><see langword="true"/> if initialization completed successfully; otherwise, <see langword="false"/>.</returns>
    bool TryInitialize(out string errorMessage);

    /// <summary>
    /// Sets the current simulation speed.
    /// </summary>
    /// <param name="speed">The new simulation speed.</param>
    void SetSpeed(SimulationSpeed speed);

    /// <summary>
    /// Pauses simulation advancement.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resumes simulation advancement.
    /// </summary>
    void Resume();

    /// <summary>
    /// Sets the paused state explicitly.
    /// </summary>
    /// <param name="paused"><see langword="true"/> to pause the simulation; otherwise, <see langword="false"/>.</param>
    void SetPaused(bool paused);

    /// <summary>
    /// Advances the simulation by an explicit number of ticks.
    /// </summary>
    /// <param name="ticks">The non-negative number of ticks to advance.</param>
    void AdvanceTicks(long ticks);

    /// <summary>
    /// Creates a snapshot of the current runtime state suitable for save/load integration.
    /// </summary>
    /// <returns>A serializable world time state snapshot.</returns>
    WorldTimeState CreateStateSnapshot();

    /// <summary>
    /// Restores the runtime state from a previously created snapshot.
    /// </summary>
    /// <param name="state">The state snapshot to restore.</param>
    void RestoreState(WorldTimeState state);
  }
}