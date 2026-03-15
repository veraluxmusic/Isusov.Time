using Isusov.Time.Config;
using System;
using UnityEngine;

namespace Isusov.Time.Scheduling
{
    /// <summary>
    /// Identifies a scheduled simulation event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="ScheduledEventHandle"/> is a lightweight value used to reference a scheduled event
    /// after registration. It can be stored by gameplay systems to support cancellation, lookups, or diagnostics.
    /// </para>
    /// <para>
    /// The handle contains a stable scheduler-generated identifier and the target tick the event was scheduled for.
    /// Equality is based on the identifier, not the target tick.
    /// </para>
    /// </remarks>
    [Serializable]
    public struct ScheduledEventHandle : IEquatable<ScheduledEventHandle>
    {
        [SerializeField] private long id;
        [SerializeField] private long targetTick;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledEventHandle"/> struct.
        /// </summary>
        /// <param name="id">The scheduler-generated event identifier. Must be greater than zero.</param>
        /// <param name="targetTick">The absolute simulation tick at which the event is intended to fire.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="id"/> is less than or equal to zero.
        /// </exception>
        public ScheduledEventHandle(long id, GameTick targetTick)
        {
            if (id <= 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Scheduled event handle id must be greater than zero.");
            }

            this.id = id;
            this.targetTick = targetTick.Value;
        }

        /// <summary>
        /// Gets the unique scheduler-generated identifier for the event.
        /// </summary>
        public readonly long Id => id;

        /// <summary>
        /// Gets the absolute simulation tick at which the event was scheduled to execute.
        /// </summary>
        public readonly GameTick TargetTick => new(targetTick);

        /// <summary>
        /// Gets a value indicating whether this handle represents a valid scheduled event reference.
        /// </summary>
        public readonly bool IsValid => id > 0L;

        /// <summary>
        /// Determines whether this instance and another handle reference the same scheduled event.
        /// </summary>
        /// <param name="other">The other handle to compare.</param>
        /// <returns><see langword="true"/> if both handles refer to the same event identifier; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(ScheduledEventHandle other)
        {
            return id == other.id;
        }

        /// <summary>
        /// Determines whether this instance and another object reference the same scheduled event.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="ScheduledEventHandle"/> with the same identifier; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object obj)
        {
            return obj is ScheduledEventHandle other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for the handle.
        /// </summary>
        /// <returns>A hash code based on the event identifier.</returns>
        public override readonly int GetHashCode()
        {
            return id.GetHashCode();
        }

        /// <summary>
        /// Returns a readable representation of the handle.
        /// </summary>
        /// <returns>
        /// A formatted identifier and target tick for valid handles, or <c>InvalidHandle</c> otherwise.
        /// </returns>
        public override readonly string ToString()
        {
            return IsValid ? $"Handle#{id}@{targetTick}" : "InvalidHandle";
        }

        /// <summary>
        /// Determines whether two handles reference the same scheduled event.
        /// </summary>
        /// <param name="left">The first handle.</param>
        /// <param name="right">The second handle.</param>
        /// <returns><see langword="true"/> if both handles reference the same identifier; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ScheduledEventHandle left, ScheduledEventHandle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two handles reference different scheduled events.
        /// </summary>
        /// <param name="left">The first handle.</param>
        /// <param name="right">The second handle.</param>
        /// <returns><see langword="true"/> if the handles reference different identifiers; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ScheduledEventHandle left, ScheduledEventHandle right)
        {
            return !left.Equals(right);
        }
    }
}