using Isusov.Time.Config;
using System;

namespace Isusov.Time.Services
{
    /// <summary>
    /// Event payload raised after the simulation clock advances by one or more ticks.
    /// </summary>
    /// <remarks>
    /// This event describes the absolute tick transition performed by the runtime.
    /// Higher-level date and season transitions are represented by separate event payloads.
    /// </remarks>
    public readonly struct TickAdvancedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TickAdvancedEvent"/> struct.
        /// </summary>
        /// <param name="previousTick">The tick value before advancement.</param>
        /// <param name="currentTick">The tick value after advancement.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="currentTick"/> is earlier than <paramref name="previousTick"/>.
        /// </exception>
        public TickAdvancedEvent(GameTick previousTick, GameTick currentTick)
        {
            if (currentTick < previousTick)
            {
                throw new ArgumentOutOfRangeException(nameof(currentTick), "Current tick cannot be earlier than previous tick.");
            }

            PreviousTick = previousTick;
            CurrentTick = currentTick;
            DeltaTicks = currentTick.Value - previousTick.Value;
        }

        /// <summary>
        /// Gets the tick value before the advance.
        /// </summary>
        public GameTick PreviousTick { get; }

        /// <summary>
        /// Gets the tick value after the advance.
        /// </summary>
        public GameTick CurrentTick { get; }

        /// <summary>
        /// Gets the number of ticks advanced during the transition.
        /// </summary>
        public long DeltaTicks { get; }
    }
}