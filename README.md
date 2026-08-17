# No Speed Limit Marking Decals

Hides the default speed-limit decals on roads while preserving other road markings.

**Tags:** Code Mod; UIPropsDecals; UIRoads; Roads

Do you get frustrated by the excessive number of speed-limit decals that the
game adds to roads by default? I found myself constantly deleting them. They
made my roads look cluttered and unrealistic, especially in dense neighborhoods
and areas with alleys.

I created this mod to remove them once and for all, and I wanted to share it
with the community in case anyone else feels the same way.

No Speed Limit Marking Decals is a visual-only code mod that hides the
automatically generated speed-limit decals on road surfaces. The mod is enabled
by default and can be toggled on/off from the in-game options menu.

Only road speed-limit decals are targeted; this mod is purely visual.

This mod does not:

- Change road or vehicle behavior.
- Change simulation values.
- Remove other road decals, such as arrows, lines, crosswalks, or stop/yield
  decals.
- Remove roadside speed-limit signs.

No Harmony patches or additional dependencies are required. The mod does not
add custom data to saved games, and disabling it returns the game to vanilla
behavior.

Built and runtime-tested in my own cities.

Although I work in IT and have a solid technical foundation, I am not a
seasoned mod developer. I used AI-assisted development tools to help expedite
parts of this mod's development.

Source code is published at
[GitHub](https://github.com/anonymousprime2020/NoSpeedLimitMarkings) for
transparency.

## Status

Version `0.1.0` is a runtime-tested release candidate. It loaded successfully
in Cities: Skylines II `1.6.0f1`, hid the targeted road speed-limit decals,
and ran alongside the user's existing mod playset during the initial
smoke test.

The public PDX Mods listing is named **No Speed Limit Marking Decals**. The
technical project, namespace, assembly, and local mod identifier remain
`NoSpeedLimitMarkings`. The v0.1.0 in-game Options page retains its original
**No Speed Limit Markings** label so this repository continues to match the
already-tested and published binary exactly.

The Release build passes against the installed `1.6.0f1`-era game/toolchain
with warnings treated as errors. Unity Entities, Colossal versioning, and Burst
post-processing completed for Windows, macOS, and Linux. Broader coverage of
all road families, lifecycle scenarios, and large-city performance remains in
progress; see `VALIDATION.md`.

## Technical approach

The game selects road speed-limit decals from the exact static-object prefab
`LaneMarkings Placeholder`. The mod removes only candidates in that placeholder
whose `TrafficSignData` includes `TrafficSignType.SpeedLimit`, before
`SecondaryObjectSystem` generates the road objects. For instances already
present in a loaded save, it tracks only matching `PrefabRef` entities and
clears their visible bit in the game's transient pre-culling output. It adds no
serialized component or city data.

Because roadside signs are not candidates in `LaneMarkings Placeholder`, they
remain untouched even though they also carry speed-limit traffic-sign data.

Turning the setting off releases tracked instances back to normal culling,
merges only the candidates removed by this mod back into the placeholder
buffer, and asks vanilla road owners to regenerate any missing secondary
objects.

## Build prerequisites

1. Install or repair the official toolchain from the game's Modding options.
2. Install the Visual Studio .NET desktop workload and .NET Framework 4.8
   targeting pack.
3. Restart the IDE or terminal so the `CSII_*` user environment variables are
   available.
4. Build the project. The official targets post-process and deploy it to the
   toolchain-defined local Mods directory.

## Local installation

1. Exit Cities: Skylines II.
2. Copy the complete `NoSpeedLimitMarkings` playable-build folder to:
   `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods`
3. Start the game and add the local mod to a dedicated test playset.
4. Load a disposable city first. Do not use the only copy of an important save.

## Test priorities

- Existing and newly placed EU and NA roads.
- Road upgrades, replacements, curves, bridges, tunnels, and intersections.
- Save/reload and enable/disable cycles.
- Confirm arrows, lane lines, crosswalks, roadside signs, speeds, zoning, and
  pathfinding are unchanged.
- Road Builder, Better Bulldozer, Town Road Lane, Anarchy/Move It, and speed
  adjustment mods.

## License

MIT. See `LICENSE`.

