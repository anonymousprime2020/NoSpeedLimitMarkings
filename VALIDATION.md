# Validation Record

## Validated

- The official project template was scaffolded from the template shipped with
  the installed game.
- Current installed `Game.dll` types and public members used by the mod
  were checked directly.
- The speed-marking path was narrowed to `LaneMarkings Placeholder` and
  `TrafficSignData` speed-limit candidates.
- Existing instances are tracked in memory and filtered only in transient
  pre-culling output rather than through persisted road data.
- Installed `BatchInstanceSystem` IL was checked: a `BatchesUpdated` entry whose
  transient `NearCamera` flag is cleared takes the vanilla `RemoveInstances`
  path, while an unmodified refresh restores normal batches.
- The render filter is registered directly before `BatchInstanceSystem` in the
  Rendering phase and uses the public pre-culling dependency contract.
- Source compilation completed with no errors against the installed game,
  Colossal, Unity ECS, Collections, and framework assemblies.
- The official Release pipeline completed with warnings treated as errors.
- Unity Entities IL post-processing, Colossal versioning, and Burst compilation
  completed for Windows, macOS, and Linux.
- The staged playable artifact contains the managed mod DLL/PDB and all three
  platform-native outputs.
- The installed game and toolchain files were not modified. To bridge the
  separate Codex sandbox account, a workspace-only copy of the official
  post-processor was changed solely to read process-scoped paths and a
  workspace Burst host copy; the official IL/Burst processors and inputs were
  otherwise used unchanged.
- The mod has no Harmony or third-party mod dependency.
- The packaged mod was deployed locally and loaded successfully in Cities:
  Skylines II `1.6.0f1`.
- An active-city smoke test confirmed that the targeted pavement speed-limit
  numerals were hidden while the surrounding road remained functional.
- The user's existing mod playset loaded alongside the mod after playset
  synchronization completed.
- The tested game session terminated normally without a game-level error.

## Not yet validated

- Visual results for every EU/NA speed value and road family.
- Save/load and disable/uninstall restoration behavior.
- Exhaustive compatibility and performance testing across supported mod
  combinations.
- The frame-time cost of synchronously completing pre-culling dependencies in
  a large city.

## Playable build status

Cities: Skylines II's official targets run a required post-processor after the
normal C# compilation. That pipeline has now completed. Install only the full
playable-build folder, not the earlier `compilecheck` DLL or a bare DLL copied
from an intermediate build directory.

