using Isusov.Time.Config;
using Isusov.Time.Core;
using System;
using UnityEngine;

namespace Isusov.Time.Services
{
  /// <summary>
  /// Serializable runtime snapshot of the world time service state.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This snapshot captures the minimal runtime state required to restore the time system
  /// to a consistent point:
  /// </para>
  /// <list type="bullet">
  /// <item><description>the current absolute simulation tick</description></item>
  /// <item><description>the current simulation speed</description></item>
  /// <item><description>whether the runtime is paused</description></item>
  /// </list>
  /// <para>
  /// It intentionally does not include authored configuration such as the calendar definition,
  /// tick mapping, or season profile. Those remain in <see cref="WorldTimeConfig"/>.
  /// </para>
  /// <para>
  /// This struct is intentionally not marked <c>readonly</c> so Unity can serialize and restore its
  /// backing fields predictably across save/load workflows.
  /// </para>
  /// </remarks>
  [Serializable]
  public struct WorldTimeState
  {
    [SerializeField] private long currentTick;
    [SerializeField] private SimulationSpeed speed;
    [SerializeField] private bool paused;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldTimeState"/> struct.
    /// </summary>
    /// <param name="currentTick">The absolute simulation tick to store.</param>
    /// <param name="speed">The simulation speed to store.</param>
    /// <param name="paused">The paused state to store.</param>
    public WorldTimeState(GameTick currentTick, SimulationSpeed speed, bool paused)
    {
      this.currentTick = currentTick.Value;
      this.speed = speed;
      this.paused = paused;
    }

    /// <summary>
    /// Gets the stored absolute simulation tick.
    /// </summary>
    public readonly GameTick CurrentTick => new(currentTick);

    /// <summary>
    /// Gets the stored simulation speed.
    /// </summary>
    public readonly SimulationSpeed Speed => speed;

    /// <summary>
    /// Gets a value indicating whether the stored runtime state is paused.
    /// </summary>
    public readonly bool Paused => paused;
  }
}