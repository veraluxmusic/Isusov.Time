using Isusov.Time.Core;
using System;
using System.Collections.Generic;

namespace Isusov.Time.Scheduling
{
    /// <summary>
    /// Schedules callbacks to execute when simulation time reaches or passes specific ticks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="SimulationScheduler"/> is intentionally driven by absolute simulation ticks rather than
    /// Unity frame time or wall-clock time. This keeps scheduling deterministic and compatible with
    /// headless simulation, save/load reconstruction, fast-forwarding, and tests.
    /// </para>
    /// <para>
    /// The scheduler does not own time progression. A higher-level runtime, typically
    /// <see cref="WorldTimeService"/>, is expected to advance a <see cref="SimulationClock"/> and then call
    /// <see cref="ExecuteDue"/> with the current tick.
    /// </para>
    /// </remarks>
    public sealed class SimulationScheduler
    {
        private sealed class ScheduledEntry
        {
            public ScheduledEntry(ScheduledEventHandle handle, Action callback)
            {
                Handle = handle;
                Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            }

            public ScheduledEventHandle Handle { get; }

            public Action Callback { get; }
        }

        private readonly SortedDictionary<long, List<ScheduledEntry>> scheduledByTick =
            new();

        private readonly Dictionary<long, long> handleToTick =
            new();

        private long nextHandleId = 1L;

        /// <summary>
        /// Gets the number of currently pending scheduled events.
        /// </summary>
        public int PendingCount => handleToTick.Count;

        /// <summary>
        /// Schedules a callback to execute when simulation reaches or passes the specified absolute tick.
        /// </summary>
        /// <param name="targetTick">The absolute simulation tick at which the callback becomes due.</param>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>A handle that can be used to identify or cancel the scheduled event.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="callback"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when the scheduler cannot allocate a new handle identifier.
        /// </exception>
        public ScheduledEventHandle ScheduleAt(GameTick targetTick, Action callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var handle = new ScheduledEventHandle(checked(nextHandleId++), targetTick);

            if (!scheduledByTick.TryGetValue(targetTick.Value, out var bucket))
            {
                bucket = new();
                scheduledByTick.Add(targetTick.Value, bucket);
            }

            bucket.Add(new ScheduledEntry(handle, callback));
            handleToTick.Add(handle.Id, targetTick.Value);

            return handle;
        }

        /// <summary>
        /// Schedules a callback to execute after a relative delay measured in simulation ticks.
        /// </summary>
        /// <param name="delay">The non-negative delay, in ticks, from <paramref name="currentTick"/>.</param>
        /// <param name="currentTick">The current absolute simulation tick.</param>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>A handle that can be used to identify or cancel the scheduled event.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="callback"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when the computed target tick or handle identifier exceeds the supported range.
        /// </exception>
        public ScheduledEventHandle ScheduleAfter(GameTick delay, GameTick currentTick, Action callback)
        {
            var targetTick = currentTick.AdvanceBy(delay.Value);
            return ScheduleAt(targetTick, callback);
        }

        /// <summary>
        /// Attempts to cancel a previously scheduled event.
        /// </summary>
        /// <param name="handle">The handle returned when the event was scheduled.</param>
        /// <returns>
        /// <see langword="true"/> if the event was found and removed; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Cancel(ScheduledEventHandle handle)
        {
            if (!handle.IsValid || !handleToTick.TryGetValue(handle.Id, out var targetTick))
            {
                return false;
            }

            if (!scheduledByTick.TryGetValue(targetTick, out var bucket))
            {
                handleToTick.Remove(handle.Id);
                return false;
            }

            for (var i = 0; i < bucket.Count; i++)
            {
                if (bucket[i].Handle.Id != handle.Id)
                {
                    continue;
                }

                bucket.RemoveAt(i);

                if (bucket.Count == 0)
                {
                    scheduledByTick.Remove(targetTick);
                }

                handleToTick.Remove(handle.Id);
                return true;
            }

            handleToTick.Remove(handle.Id);
            return false;
        }

        /// <summary>
        /// Executes all callbacks whose target tick is less than or equal to the supplied current tick.
        /// </summary>
        /// <param name="currentTick">The current absolute simulation tick.</param>
        /// <returns>The number of callbacks executed.</returns>
        /// <remarks>
        /// <para>
        /// Events scheduled for the same tick execute in the order they were inserted into that tick bucket.
        /// </para>
        /// <para>
        /// If a callback throws, the exception propagates to the caller and execution stops immediately.
        /// Any events already removed from the scheduler remain removed.
        /// </para>
        /// </remarks>
        public int ExecuteDue(GameTick currentTick)
        {
            var executedCount = 0;

            while (scheduledByTick.Count > 0)
            {
                var enumerator = scheduledByTick.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    break;
                }

                var firstPair = enumerator.Current;
                if (firstPair.Key > currentTick.Value)
                {
                    break;
                }

                scheduledByTick.Remove(firstPair.Key);

                for (var i = 0; i < firstPair.Value.Count; i++)
                {
                    var entry = firstPair.Value[i];
                    handleToTick.Remove(entry.Handle.Id);
                    entry.Callback.Invoke();
                    executedCount++;
                }
            }

            return executedCount;
        }

        /// <summary>
        /// Removes all pending scheduled events.
        /// </summary>
        public void Clear()
        {
            scheduledByTick.Clear();
            handleToTick.Clear();
        }

        /// <summary>
        /// Determines whether a handle still refers to a pending scheduled event.
        /// </summary>
        /// <param name="handle">The handle to test.</param>
        /// <returns>
        /// <see langword="true"/> if the handle currently refers to a pending event; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(ScheduledEventHandle handle)
        {
            return handle.IsValid && handleToTick.ContainsKey(handle.Id);
        }
    }
}