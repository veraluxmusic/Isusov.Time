# Isusov.Time

A focused Unity package for deterministic in-game time, calendars, seasons, and scheduled simulation events.

`Isusov.Time` is designed for games that need more than a simple clock. It gives you a configurable calendar model, tick-based time progression, date mapping, seasonal resolution, and a scheduler for gameplay events that should execute at precise simulation ticks.

## Features

- Tick-based simulation clock
- Configurable real-seconds-to-tick mapping
- Calendar/date conversion utilities
- Season resolution support
- Scheduled callbacks at exact simulation ticks
- World time service for Unity integration
- Strongly typed domain objects such as game dates and ticks
- Event-driven time progression hooks for gameplay systems

## Package Overview

The package is centered around these concepts:

- **GameTick** — the smallest simulation time unit
- **GameDate** — a calendar date in the game world
- **SimulationClock** — advances the world in ticks
- **TimeMapping** — converts between ticks and dates
- **SimulationScheduler** — executes callbacks at scheduled ticks
- **WorldTimeRuntime** — pure simulation runtime that owns deterministic time state and transitions
- **WorldTimeService** — Unity-facing service that coordinates time progression
- **SeasonResolver / SeasonProfile** — maps dates to seasons

This structure makes the package suitable for:

- farming and life sims
- survival games
- strategy games
- city builders
- narrative games with calendar-driven progression
- any system where gameplay depends on day/month/season transitions

## Installation

### Unity Package Manager (Git URL)

Add the package from a Git URL in the Unity Package Manager:

```text
https://github.com/veraluxmusic/Isusov.Time.git
````

Or add it manually to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.isusov.time": "https://github.com/veraluxmusic/Isusov.Time.git"
  }
}
```

## Requirements

* Unity 6 or newer

## Folder Structure

```text
Runtime/
  Calendar/
  Core/
  Scheduling/
  Seasons/
  Services/

Editor/
Tests/
```

## Quick Start

### 1. Create a world time configuration

Create and configure a `WorldTimeConfig` asset in your project.

Use it to define:

* calendar rules
* start date
* tick settings
* simulation speed
* season profile

### 2. Add `WorldTimeService` to a GameObject

Attach the service to a scene object and assign the config.

The inspectors for `WorldTimeConfig` and `WorldTimeService` now surface validation feedback directly in the Unity Editor so missing config, invalid season boundaries, and bootstrap risks show up before you hit Play.

### 3. Initialize and use the service

Example:

```csharp
using UnityEngine;
using Isusov.Time.Services;

public sealed class TimeBootstrap : MonoBehaviour
{
    [SerializeField] private WorldTimeService worldTimeService;

    private void Awake()
    {
        if (worldTimeService == null)
        {
            Debug.LogError("WorldTimeService reference is missing.");
            enabled = false;
            return;
        }

        if (!worldTimeService.TryInitialize(out var errorMessage))
        {
            Debug.LogError(errorMessage, worldTimeService);
            enabled = false;
        }
    }
}
```

## Example: Reading Current Time

```csharp
using UnityEngine;
using Isusov.Time.Services;

public sealed class TimeReader : MonoBehaviour
{
    [SerializeField] private WorldTimeService worldTimeService;

    private void Start()
    {
        if (!worldTimeService.TryInitialize(out var errorMessage))
        {
            Debug.LogError(errorMessage, worldTimeService);
            enabled = false;
            return;
        }

        Debug.Log($"Current tick: {worldTimeService.CurrentTick}");
        Debug.Log($"Current date: {worldTimeService.CurrentDate}");
        Debug.Log($"Current season: {worldTimeService.CurrentSeason}");
    }
}
```

## Example: Scheduling Gameplay Logic

```csharp
using UnityEngine;
using Isusov.Time.Services;

public sealed class CropGrowthSystem : MonoBehaviour
{
    [SerializeField] private WorldTimeService worldTimeService;

    private void Start()
    {
        if (!worldTimeService.TryInitialize(out var errorMessage))
        {
            Debug.LogError(errorMessage, worldTimeService);
            enabled = false;
            return;
        }

        var targetTick = worldTimeService.CurrentTick.AdvanceBy(240);
        worldTimeService.ScheduleAt(targetTick, OnCropGrowthReady);
    }

    private void OnCropGrowthReady()
    {
        Debug.Log("Crop growth completed.");
    }
}
```

## Example: Reacting to Day Changes

```csharp
using UnityEngine;
using Isusov.Time.Services;

public sealed class DailyResetSystem : MonoBehaviour
{
    [SerializeField] private WorldTimeService worldTimeService;

    private void OnEnable()
    {
        worldTimeService.DayChanged += OnDayChanged;
    }

    private void OnDisable()
    {
        worldTimeService.DayChanged -= OnDayChanged;
    }

    private void OnDayChanged(DayChangedEvent change)
    {
        Debug.Log($"New day: {change.CurrentDate}");
    }
}
```

## Typical Use Cases

### Farming / Life Simulation

Use daily and seasonal transitions for crops, shops, NPC routines, and festivals.

### Strategy / Colony Simulation

Drive production cycles, weather windows, and long-running world events.

### Narrative Systems

Lock or unlock quests, dialogue, or story beats by exact date or season.

### Survival Mechanics

Control migration, spawning, temperatures, or resource generation over time.

## Design Goals

This package aims to be:

* deterministic
* explicit
* easy to reason about
* gameplay-oriented
* decoupled from frame rate
* flexible enough for custom world calendars

## Best Practices

* Initialize `WorldTimeService` once during scene bootstrap
* Prefer `TryInitialize(out errorMessage)` in scene code that should fail gracefully
* In gameplay code, depend on the narrowest runtime contract that fits the job: `IWorldTimeReader` for reads, `IWorldTimeEventSource` for subscriptions, `IWorldTimeScheduler` for deferred work, and `IWorldTimeController` for orchestration
* Subscribe to `TransitionOccurred` when you need a single broad transition stream; use the specialized events when you only care about one transition type
* Use the inspector validation messages on `WorldTimeConfig` and `WorldTimeService` to catch authoring errors early
* Keep time progression authoritative in a single service
* Prefer `WorldTimeRuntime` for tests and non-MonoBehaviour orchestration; keep `WorldTimeService` as the Unity scene host
* Use scheduled ticks for gameplay-critical timing
* Prefer domain events like day/month/season transitions over ad hoc polling
* Treat calendar definition and mapping as content, not hardcoded logic

## Testing

The package is structured to support edit-mode and runtime tests for:

* date conversion
* tick advancement
* scheduling
* season resolution
* season boundary and year rollover transitions
* event emission across time boundaries

## Roadmap

Planned polish areas include:

* fuller test coverage
* CI automation
* expanded documentation
* sample scenes
* package metadata cleanup
* release workflow improvements

## Contributing

Contributions are welcome. A dedicated contribution guide can be added as the package matures.

When contributing, keep changes:

* focused
* documented
* tested
* consistent with the existing public API style

## License

This package is released under the [CC0 1.0 Universal](LICENSE) public domain dedication.

## Status

Early package foundation with core time-system building blocks in place and ready for hardening, testing, and production polish.

---