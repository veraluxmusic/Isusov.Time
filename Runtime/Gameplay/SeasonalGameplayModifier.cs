using Isusov.Time.Seasons;
using System;
using UnityEngine;

namespace Isusov.Time.Gameplay
{
    /// <summary>
    /// Associates a named gameplay modifier key with a season-aware numeric modifier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type is a thin authoring container intended for higher-level gameplay systems that need
    /// to expose multiple season-sensitive values by semantic key, such as:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>BiomeYield</c></description></item>
    /// <item><description><c>CropGrowthRate</c></description></item>
    /// <item><description><c>WildlifeSpawnWeight</c></description></item>
    /// </list>
    /// <para>
    /// The time package does not interpret the key. It simply stores it and delegates numeric resolution
    /// to the contained <see cref="SeasonalValueModifier"/>.
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class SeasonalGameplayModifier
    {
        [SerializeField] private string key = string.Empty;
        [SerializeField] private SeasonalValueModifier modifier = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonalGameplayModifier"/> class.
        /// </summary>
        /// <param name="key">The semantic gameplay key associated with this modifier.</param>
        /// <param name="modifier">The season-aware numeric modifier definition.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modifier"/> is <see langword="null"/>.
        /// </exception>
        public SeasonalGameplayModifier(string key, SeasonalValueModifier modifier)
        {
            this.key = key ?? string.Empty;
            this.modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
        }

        /// <summary>
        /// Gets the semantic gameplay key associated with this modifier entry.
        /// </summary>
        public string Key => key;

        /// <summary>
        /// Gets the underlying season-aware numeric modifier.
        /// </summary>
        public SeasonalValueModifier Modifier => modifier;

        /// <summary>
        /// Applies the season-specific multiplier to a supplied base value.
        /// </summary>
        /// <param name="season">The season to resolve against.</param>
        /// <param name="baseValue">The unmodified gameplay value.</param>
        /// <returns>The value after the season-specific multiplier has been applied.</returns>
        public float Apply(Season season, float baseValue)
        {
            return modifier.Apply(season, baseValue);
        }
    }
}