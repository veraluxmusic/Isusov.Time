# Game Time

The `Game Time` package provides deterministic in-game time for Unity projects that need more than a simple real-time clock. Use it to drive authored calendars, in-game dates, seasons, and scheduled simulation events from a single authoritative runtime service.

The package is suitable for farming games, colony sims, strategy games, survival loops, or any project where gameplay depends on exact day, month, year, or season transitions.

# Installing Game Time

Install the package with the Unity Package Manager by using the Git URL or a package dependency entry, as described in the [Package Manager documentation](https://docs.unity3d.com/Manual/upm-ui-giturl.html).

Git URL:

`https://github.com/veraluxmusic/Isusov.Time.git`

No additional packages are required.

<a name="UsingGameTime"></a>
# Using Game Time

## Recommended setup

1. Create a `WorldTimeConfig` asset.
2. Configure the calendar, starting date, tick pacing, and optional season profile.
3. Add `WorldTimeService` to a scene object that acts as the authoritative time owner.
4. Assign the configuration asset to the component.
5. Either leave `Auto Initialize` enabled or call `TryInitialize(out errorMessage)` during your scene bootstrap.

`TryInitialize` is recommended for gameplay bootstrap code because it lets a scene fail gracefully when setup is incomplete. The `Initialize` method still throws for explicit fail-fast workflows.

The inspectors for `WorldTimeConfig` and `WorldTimeService` surface validation status directly in the Unity Editor so authoring mistakes are visible before entering play mode.

## Core runtime concepts

- `GameTick` is the absolute simulation timeline.
- `GameDate` is the resolved calendar date.
- `SimulationClock` advances the timeline from real elapsed time.
- `TimeMapping` converts ticks into day indices, dates, and date start ticks.
- `SimulationScheduler` runs callbacks when specific ticks become due.
- `WorldTimeService` is the Unity-facing composition root.

## Typical workflow

### Read the current world time

Use `CurrentTick`, `CurrentDate`, `CurrentSeason`, or `CurrentGameTime` when you need a snapshot of the current simulation state.

### React to time transitions

Subscribe to `DayChanged`, `MonthChanged`, `YearChanged`, `SeasonChanged`, or `TickAdvanced` to drive gameplay systems without per-frame polling.

Subscribe in `OnEnable` and unsubscribe in `OnDisable` so listeners follow normal Unity lifecycle rules.

### Schedule gameplay work

Use `ScheduleAt` for absolute target ticks and `ScheduleAfter` for relative delays. Use `CancelScheduledEvent`, `PendingScheduledEventCount`, and `IsScheduledEventPending` to manage or inspect scheduled work.

### Save and load

Use `CreateStateSnapshot()` to capture the current tick, speed, and pause state. Restore those values with `RestoreState(state)` after reconstructing the same authored configuration.

`RestoreState` clears pending scheduled callbacks because callback delegates are not serialized as part of `WorldTimeState`.

## Performance notes

- Tick advancement is deterministic and frame-rate independent.
- `WorldTimeService` emits one event per crossed boundary, so large fast-forwards scale with the number of crossed days.
- `SimulationScheduler` executes due work in target-tick order.
- Prefer the event model over repeated polling in `Update` for gameplay systems.

# Technical details

## Requirements

- Unity 6 or newer

## Known limitations

- Scheduled callbacks are not part of save-state snapshots and must be recreated after loading.
- Large single-frame time jumps emit each intermediate day transition in sequence, which is correct but can be expensive if abused.

## Package contents

| Location              | Description                                                                    |
| --------------------- | ------------------------------------------------------------------------------ |
| `Runtime/Calendar/`   | Calendar definitions, date value types, and leap-year rules.                   |
| `Runtime/Core/`       | Core simulation primitives such as ticks, clock advancement, and time mapping. |
| `Runtime/Seasons/`    | Season domain types and season resolution logic.                               |
| `Runtime/Scheduling/` | Deterministic tick-based scheduling primitives.                                |
| `Runtime/Services/`   | Unity-facing service API, events, and runtime state snapshot support.          |
| `Tests/Runtime/`      | Runtime regression tests for calendar mapping and world time behavior.         |

## Document revision history

| Date       | Reason                                                                                          |
| ---------- | ----------------------------------------------------------------------------------------------- |
| 2026-03-15 | Documentation rewritten from template placeholder to package-specific setup and usage guidance. |