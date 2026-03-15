using Isusov.Time.Seasons;
using System;
using UnityEngine;

namespace Isusov.Time.Gameplay
{
    /// <summary>
    /// Applies season-specific multipliers to a numeric gameplay value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type is intentionally generic. It does not know anything about crops, weather, biome output,
    /// economy balance, or any other gameplay domain. It simply maps a <see cref="Season"/> to a multiplier.
    /// </para>
    /// <para>
    /// The usual integration pattern is:
    /// </para>
    /// <list type="number">
    /// <item><description>Resolve the current season from <see cref="WorldTimeService"/>.</description></item>
    /// <item><description>Apply the multiplier to a domain-specific base value.</description></item>
    /// </list>
    /// </remarks>
    [Serializable]
    public sealed class SeasonalValueModifier
    {
        [SerializeField] private float noneMultiplier = 1f;
        [SerializeField] private float springMultiplier = 1f;
        [SerializeField] private float summerMultiplier = 1f;
        [SerializeField] private float autumnMultiplier = 1f;
        [SerializeField] private float winterMultiplier = 1f;

        /// <summary>
        /// Gets the multiplier used when no season is defined.
        /// </summary>
        public float NoneMultiplier => noneMultiplier;

        /// <summary>
        /// Gets the multiplier used during spring.
        /// </summary>
        public float SpringMultiplier => springMultiplier;

        /// <summary>
        /// Gets the multiplier used during summer.
        /// </summary>
        public float SummerMultiplier => summerMultiplier;

        /// <summary>
        /// Gets the multiplier used during autumn.
        /// </summary>
        public float AutumnMultiplier => autumnMultiplier;

        /// <summary>
        /// Gets the multiplier used during winter.
        /// </summary>
        public float WinterMultiplier => winterMultiplier;

        /// <summary>
        /// Gets the multiplier associated with the supplied season.
        /// </summary>
        /// <param name="season">The season whose multiplier should be returned.</param>
        /// <returns>The multiplier configured for <paramref name="season"/>.</returns>
        public float GetMultiplier(Season season)
        {
            return season switch
            {
                Season.Spring => springMultiplier,
                Season.Summer => summerMultiplier,
                Season.Autumn => autumnMultiplier,
                Season.Winter => winterMultiplier,
                _ => noneMultiplier,
            };
        }

        /// <summary>
        /// Applies the multiplier for the supplied season to a base value.
        /// </summary>
        /// <param name="season">The season that determines which multiplier is used.</param>
        /// <param name="baseValue">The unmodified gameplay value.</param>
        /// <returns>The multiplied value.</returns>
        public float Apply(Season season, float baseValue)
        {
            return baseValue * GetMultiplier(season);
        }
    }
}