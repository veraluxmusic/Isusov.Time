using System;
using UnityEngine;

namespace Isusov.Time.Core
{
  /// <summary>
  /// Represents a simulation speed preset or custom multiplier.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The multiplier is applied by <see cref="SimulationClock"/> to determine how quickly
  /// real elapsed time is converted into simulation ticks.
  /// </para>
  /// <para>
  /// This type is serializable so it can be authored in Unity assets and components while still
  /// behaving like a lightweight value object in runtime and test code.
  /// </para>
  /// <para>
  /// This struct is intentionally not marked <c>readonly</c> to preserve broad Unity serialization
  /// compatibility across editor/runtime tooling paths where mutable backing fields are required.
  /// Public API access remains immutable through readonly properties.
  /// </para>
  /// </remarks>
  [Serializable]
  public struct SimulationSpeed : IEquatable<SimulationSpeed>
  {
    [SerializeField] private string label;
    [SerializeField] private float multiplier;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationSpeed"/> struct.
    /// </summary>
    /// <param name="label">A human-readable label such as <c>1x</c> or <c>Fast Forward</c>.</param>
    /// <param name="multiplier">The speed multiplier. Must be greater than or equal to zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="multiplier"/> is negative.
    /// </exception>
    public SimulationSpeed(string label, float multiplier)
    {
      if (multiplier < 0f)
      {
        throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "Simulation speed multiplier cannot be negative.");
      }

      this.label = label ?? string.Empty;
      this.multiplier = multiplier;
    }

    /// <summary>
    /// Gets the display label for the speed preset.
    /// </summary>
    /// <remarks>
    /// If no explicit label was authored, <c>Custom</c> is returned.
    /// </remarks>
    public readonly string Label => string.IsNullOrWhiteSpace(label) ? "Custom" : label;

    /// <summary>
    /// Gets the numeric speed multiplier.
    /// </summary>
    /// <remarks>
    /// A value of <c>0</c> represents a paused speed preset.
    /// </remarks>
    public readonly float Multiplier => multiplier;

    /// <summary>
    /// Gets a predefined paused speed.
    /// </summary>
    public static SimulationSpeed Paused => new("Paused", 0f);

    /// <summary>
    /// Gets a predefined normal speed.
    /// </summary>
    public static SimulationSpeed OneX => new("1x", 1f);

    /// <summary>
    /// Gets a predefined 2x speed.
    /// </summary>
    public static SimulationSpeed TwoX => new("2x", 2f);

    /// <summary>
    /// Gets a predefined 5x speed.
    /// </summary>
    public static SimulationSpeed FiveX => new("5x", 5f);

    /// <summary>
    /// Gets a predefined 10x speed.
    /// </summary>
    public static SimulationSpeed TenX => new("10x", 10f);

    /// <summary>
    /// Determines whether this speed represents a paused multiplier.
    /// </summary>
    public readonly bool IsPaused => Mathf.Approximately(multiplier, 0f);

    /// <summary>
    /// Determines whether this instance and another speed have the same label and multiplier.
    /// </summary>
    /// <param name="other">The other speed to compare.</param>
    /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
    public readonly bool Equals(SimulationSpeed other)
    {
      return string.Equals(Label, other.Label, StringComparison.Ordinal) &&
             multiplier == other.multiplier;
    }

    /// <summary>
    /// Determines whether this instance and another object represent the same speed value.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="SimulationSpeed"/> with the same value; otherwise, <see langword="false"/>.</returns>
    public override readonly bool Equals(object obj)
    {
      return obj is SimulationSpeed other && Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the speed value.
    /// </summary>
    /// <returns>A hash code based on the label and multiplier.</returns>
    public override readonly int GetHashCode()
    {
      unchecked
      {
        return ((Label != null ? StringComparer.Ordinal.GetHashCode(Label) : 0) * 397) ^ multiplier.GetHashCode();
      }
    }

    /// <summary>
    /// Returns a readable representation of the simulation speed.
    /// </summary>
    /// <returns>A formatted label and multiplier pair.</returns>
    public override readonly string ToString()
    {
      return $"{Label} ({Multiplier:0.###}x)";
    }

    /// <summary>
    /// Determines whether two speed values are equal.
    /// </summary>
    /// <param name="left">The first speed.</param>
    /// <param name="right">The second speed.</param>
    /// <returns><see langword="true"/> if both speeds are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(SimulationSpeed left, SimulationSpeed right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two speed values are not equal.
    /// </summary>
    /// <param name="left">The first speed.</param>
    /// <param name="right">The second speed.</param>
    /// <returns><see langword="true"/> if the speeds differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(SimulationSpeed left, SimulationSpeed right)
    {
      return !left.Equals(right);
    }
  }
}