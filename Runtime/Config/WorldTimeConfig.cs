using Isusov.Time.Calendar;
using Isusov.Time.Core;
using Isusov.Time.Seasons;
using System;
using UnityEngine;

namespace Isusov.Time.Config
{
    /// <summary>
    /// Unity-authored configuration for the world time subsystem.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This asset defines the static inputs used to construct the runtime time system:
    /// calendar rules, start date, tick pacing, default speed, pause state, and optional season profile.
    /// </para>
    /// <para>
    /// The asset is intentionally declarative. Runtime state belongs in <see cref="WorldTimeService"/>,
    /// while deterministic time logic remains in the pure domain types such as
    /// <see cref="CalendarDefinition"/>, <see cref="SimulationClock"/>, and <see cref="TimeMapping"/>.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(fileName = "WorldTimeConfig", menuName = "Isusov/Time/World Time Config")]
    public sealed class WorldTimeConfig : ScriptableObject
    {
        [Header("Calendar")]
        [SerializeField] private CalendarDefinition calendarDefinition = null;

        [Header("Start Date")]
        [Min(1)]
        [SerializeField] private int startYear = 1;

        [Min(1)]
        [SerializeField] private int startMonth = 1;

        [Min(1)]
        [SerializeField] private int startDay = 1;

        [Header("Tick Mapping")]
        [Min(1)]
        [SerializeField] private int ticksPerDay = 60;

        [Min(0.000001f)]
        [SerializeField] private double realSecondsPerTick = 1d;

        [Header("Runtime Defaults")]
        [SerializeField] private SimulationSpeed defaultSimulationSpeed = default;
        [SerializeField] private bool startPaused;

        [Header("Seasons")]
        [SerializeField] private SeasonProfile seasonProfile = null;

        /// <summary>
        /// Gets the calendar definition used by the runtime.
        /// </summary>
        /// <remarks>
        /// If no calendar was explicitly authored, a default Gregorian calendar is created lazily.
        /// </remarks>
        public CalendarDefinition CalendarDefinition => calendarDefinition ??= CalendarDefinition.CreateDefaultGregorian();

        /// <summary>
        /// Gets the configured simulation start date.
        /// </summary>
        public GameDate StartDate => new(startYear, startMonth, startDay);

        /// <summary>
        /// Gets the number of simulation ticks that compose a single in-game day.
        /// </summary>
        public int TicksPerDay => ticksPerDay;

        /// <summary>
        /// Gets the real-world duration, in seconds, represented by a single simulation tick at 1x speed.
        /// </summary>
        public double RealSecondsPerTick => realSecondsPerTick;

        /// <summary>
        /// Gets the default simulation speed used when the runtime is created.
        /// </summary>
        /// <remarks>
        /// If the authored value is uninitialized, <see cref="SimulationSpeed.OneX"/> is returned.
        /// </remarks>
        public SimulationSpeed DefaultSimulationSpeed =>
            defaultSimulationSpeed.Multiplier <= 0f && string.IsNullOrWhiteSpace(defaultSimulationSpeed.Label)
                ? SimulationSpeed.OneX
                : defaultSimulationSpeed;

        /// <summary>
        /// Gets a value indicating whether the simulation starts paused.
        /// </summary>
        public bool StartPaused => startPaused;

        /// <summary>
        /// Gets the optional season profile used to resolve seasons from dates.
        /// </summary>
        public SeasonProfile SeasonProfile => seasonProfile;

        /// <summary>
        /// Validates the authored configuration and throws if any invariant is violated.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when tick pacing is invalid, the calendar is invalid, or the start date is not valid for the calendar.
        /// </exception>
        public void ValidateOrThrow()
        {
            CalendarDefinition.ValidateOrThrow();
            CalendarDefinition.EnsureValidGameDate(StartDate);

            if (ticksPerDay <= 0)
            {
                throw new InvalidOperationException("WorldTimeConfig.TicksPerDay must be greater than zero.");
            }

            if (realSecondsPerTick <= 0d)
            {
                throw new InvalidOperationException("WorldTimeConfig.RealSecondsPerTick must be greater than zero.");
            }

            seasonProfile?.ValidateOrThrow(CalendarDefinition);
        }

        /// <summary>
        /// Creates a new <see cref="SimulationClock"/> using this configuration and the supplied initial tick.
        /// </summary>
        /// <param name="initialTick">The initial simulation tick for the clock.</param>
        /// <returns>A configured simulation clock instance.</returns>
        public SimulationClock CreateClock(GameTick initialTick)
        {
            ValidateOrThrow();

            var speed = DefaultSimulationSpeed.Multiplier <= 0f && !startPaused
                ? SimulationSpeed.OneX
                : DefaultSimulationSpeed;

            return new SimulationClock(realSecondsPerTick, initialTick, speed, startPaused);
        }

        /// <summary>
        /// Creates a new <see cref="TimeMapping"/> using this configuration.
        /// </summary>
        /// <returns>A configured time mapping instance.</returns>
        public TimeMapping CreateTimeMapping()
        {
            ValidateOrThrow();
            return new TimeMapping(CalendarDefinition, StartDate, ticksPerDay);
        }

        /// <summary>
        /// Restores sensible defaults when the asset is first created or reset in the Unity editor.
        /// </summary>
        private void Reset()
        {
            calendarDefinition = CalendarDefinition.CreateDefaultGregorian();
            startYear = 1;
            startMonth = 1;
            startDay = 1;
            ticksPerDay = 60;
            realSecondsPerTick = 1d;
            defaultSimulationSpeed = SimulationSpeed.OneX;
            startPaused = false;
            seasonProfile = null;
        }

        /// <summary>
        /// Performs lightweight editor-time normalization of authored values.
        /// </summary>
        private void OnValidate()
        {
            if (startYear < 1)
            {
                startYear = 1;
            }

            if (startMonth < 1)
            {
                startMonth = 1;
            }

            if (startDay < 1)
            {
                startDay = 1;
            }

            if (ticksPerDay < 1)
            {
                ticksPerDay = 1;
            }

            if (realSecondsPerTick <= 0d)
            {
                realSecondsPerTick = 1d;
            }
        }
    }
}