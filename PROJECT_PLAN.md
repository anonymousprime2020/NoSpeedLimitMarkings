# No Speed Limit Marking Decals — Approved Project Plan

## Objective

Create a visual-only Cities: Skylines II code mod that hides the automatically
generated speed-limit numerals painted on road surfaces by default.

The mod must preserve actual speed limits, vehicle behavior, road arrows, lane
lines, crosswalks, stop/yield markings, roadside signs, network connectivity,
zoning, and pathfinding.

## Scope decisions

- Public name: `No Speed Limit Marking Decals`.
- Initial public version: `0.1.0`.
- License: MIT.
- UI: native game settings only; no custom web UI.
- Dependencies: none beyond the official Cities: Skylines II code-mod SDK.
- Initial road support: vanilla and official DLC roads that use the game's
  `LaneMarkings Placeholder` generation path.
- Custom-road support: conservative compatibility first, followed by explicit
  Road Builder and other custom-road testing.
- Distribution: local testing before any Paradox Mods publication.

## Verified implementation strategy

The installed game build generates pavement speed numerals as secondary static
objects. It selects them from the exact `LaneMarkings Placeholder` prefab.

The prototype therefore:

1. Resolves only `LaneMarkings Placeholder` through `PrefabSystem`.
2. Inspects that placeholder's `PlaceholderObjectElement` candidates.
3. Classifies speed-number candidates with `TrafficSignData` and
   `TrafficSignType.SpeedLimit`.
4. Removes those candidates before vanilla `SecondaryObjectSystem` chooses
   secondary road objects.
5. Tracks already-instantiated objects only when their `PrefabRef` matches the
   exact candidate set discovered in step 3, then filters them from transient
   pre-culling output.
6. Releases tracked instances and restores only entries removed by this mod
   when the option is disabled, then asks vanilla road owners to regenerate
   anything missing.

This two-part classification is important: roadside pole signs also use the
speed-limit traffic-sign type, but they are not candidates in the lane-marking
placeholder and are therefore left alone.

## Delivery phases

### Phase 0 — Environment audit

Status: complete.

- Confirm installed game build and store location.
- Confirm IDE, SDK, Unity, and official template prerequisites.
- Identify missing toolchain initialization and targeting pack.

### Phase 1 — Game-data discovery

Status: complete.

- Identify the system that generates pavement speed numerals.
- Verify the exact placeholder, components, prefab identities, and update phase.
- Demonstrate a selector that distinguishes speed numerals from other markings
  and roadside signs.

Exit criterion: speed numerals can be selected without name-only matching or a
broad traffic-sign query.

### Phase 2 — Source prototype

Status: complete.

- Add the `IMod` entry point and native settings.
- Add pre-generation candidate suppression.
- Add render-only loaded-save filtering for exact target prefabs.
- Add reversible candidate restoration and batched owner refresh.
- Add English localization, logs, README, license, and publishing metadata.

Exit criterion: the project passes a direct C# compile/API check against the
installed game assemblies.

### Phase 3 — Official build and first in-game test

Status: complete.

- Initialize or repair the official modding toolchain in the game.
- Install the .NET Framework 4.8 targeting pack.
- Build with the official targets so the required mod post-processor runs.
- Add the local mod to a dedicated test playset.
- Verify existing and newly placed roads in a disposable city.

Exit criterion: speed numerals disappear, all protected visuals and simulation
behavior remain unchanged, and the game log has no mod exceptions.

### Phase 4 — Reversibility and save safety

Status: pending.

- Toggle the option off and on repeatedly.
- Save/reload with the option in each state.
- Add the mod to an existing test save.
- Load after disabling the mod and after removing it from the playset.
- Verify regenerated markings reflect current road speeds.

Exit criterion: no permanent city-data damage and vanilla markings recover when
the mod is disabled or removed.

### Phase 5 — Compatibility and performance beta

Status: pending.

- Test base-game and official DLC road families, EU/NA themes, left/right-hand
  traffic, intersections, roundabouts, bridges, elevated roads, and tunnels.
- Test road upgrades, replacements, speed changes, and network edits.
- Test Road Builder, Better Bulldozer, Town Road Lane, Anarchy/Move It, and a
  current speed-adjustment mod both individually and in a representative stack.
- Profile initial reconciliation and steady-state behavior in a mature city.
- Measure the synchronous pre-culling completion cost; replace the main-thread
  filter with a dependency-aware job if it is material.

Exit criterion: no continuous full-city scan, no recurring allocation churn,
no visible respawn flicker, and unknown custom candidates are handled
conservatively.

### Phase 6 — Release candidate and publication

Status: in progress.

- Maintain the release thumbnail and before/after screenshot.
- Maintain compatibility notes, recovery guidance, version, and changelog.
- Run a clean-release and subscribed-artifact smoke test.
- Publish to Paradox Mods only after explicit approval.

## Acceptance test matrix

| Area | Required result |
| --- | --- |
| Existing roads | Painted speed numerals are removed after load. |
| Newly built roads | Painted speed numerals do not appear. |
| Other markings | Arrows, dividers, crosswalks, and stop/yield markings remain. |
| Roadside signs | Pole-mounted speed-limit signs remain. |
| Simulation | Actual limits, routing, and vehicle behavior are unchanged. |
| Road editing | Upgrades, replacements, moves, and speed changes stay clean. |
| Toggle off | Vanilla numerals regenerate without rebuilding the roads manually. |
| Save lifecycle | Save/load and mod removal do not damage the road network. |
| Compatibility | Unknown/custom assets are skipped rather than broadly deleted. |
| Performance | No whole-city scan runs every simulation frame. |

## Release gates

Ongoing release validation includes the following checks:

- Official post-processed build loads successfully.
- EU and NA speed-number sets are removed selectively.
- Disable/uninstall restoration works on a copied test save.
- No gameplay or save-load exceptions appear in `Player.log`.
- Compatibility and performance tests pass on the supported game build.

