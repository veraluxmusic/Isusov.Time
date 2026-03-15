# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog,
and this project follows Semantic Versioning.

## [Unreleased]

### Added
- Initial public package structure for deterministic in-game time systems
- Tick-based simulation time model
- Calendar and game date domain types
- Date and tick mapping utilities
- Seasonal resolution support
- Scheduling primitives for simulation events
- Unity-facing world time service integration
- Initial README documentation
- Package metadata polish
- Safe `TryInitialize` flow on `WorldTimeService` for scene bootstrap code
- Scheduler query surface on `IWorldTimeService` for pending-event inspection
- Initial runtime regression tests for calendar mapping and world-time service behavior
- `WorldTimeConfig.TryValidate(out errorMessage)` for non-throwing authoring validation
- Custom editor inspectors for `WorldTimeConfig` and `WorldTimeService` with validation feedback and runtime snapshot display
- Additional runtime regression tests for season resolution, season transitions, invalid season profiles, and year rollover
- Runtime regression tests covering implicit default calendar fallback without serialized-state mutation
- Role-based world-time interfaces (`IWorldTimeReader`, `IWorldTimeController`, `IWorldTimeScheduler`, `IWorldTimeEventSource`) for lower-coupling consumers
- Pure `WorldTimeRuntime` core plus runtime-focused regression tests for non-MonoBehaviour simulation hosting
- Unified `WorldTimeTransitionEvent` stream (`TransitionOccurred`) spanning tick, date, season, speed, and pause transitions

### Changed
- Improved package presentation and discoverability
- Clarified installation and usage guidance
- Standardized package manifest fields and external documentation links
- `WorldTimeService` auto-initialization now logs and disables the component instead of throwing from `Awake`
- Package documentation now reflects actual setup and usage instead of template placeholder content
- `WorldTimeConfig` now resolves its implicit default calendar without mutating serialized asset state
- Custom inspectors now render explicit serialized fields instead of relying on `DrawDefaultInspector()`
- Public event payloads and `WorldTimeState` now use explicit constructors for clearer Unity/package compatibility
- `IWorldTimeService` now composes smaller role-based interfaces so consumers can depend on narrower seams without losing the existing aggregate surface
- `WorldTimeService` now acts as a thinner Unity host over `WorldTimeRuntime` instead of owning all simulation logic directly
- `WorldTimeService` event relay now uses explicit backing delegates and non-readonly serialized runtime flags for safer Unity authoring/runtime behavior

## [0.1.0] - 2026-03-15

### Added
- First tagged package release
- Core time simulation foundation
- Calendar/date abstractions
- Time scheduling support
- Season support
- Unity package manifest
- Project documentation baseline