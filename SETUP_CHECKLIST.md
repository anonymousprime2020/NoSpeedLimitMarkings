# Local Setup Checklist

The one-time toolchain setup and playable Release build are complete on this
PC. Keep this checklist for rebuilding after a game or SDK update.

## Already confirmed on this PC

- Cities: Skylines II is installed through Steam.
- The installed files are from the current `1.6.0f1`-era toolchain.
- Visual Studio 2022 Community is installed.
- .NET SDK 8 is installed.
- The matching Unity editor is installed.
- The source passes a direct compiler/API check against the installed game
  assemblies.
- The .NET Framework 4.8 targeting pack is installed.
- The official Unity mod project and package cache are initialized.
- The Release build and required cross-platform post-processing complete.

## One-time actions completed

1. Start Cities: Skylines II on the normal public branch.
2. Open **Options > Modding**.
3. Choose the automatic **Install** or **Repair Toolchain** action.
4. Let every official component finish and restart the game if prompted.
5. Open **Visual Studio Installer** for Visual Studio 2022 Community.
6. Add the **.NET desktop development** workload. Confirm that the **.NET
   Framework 4.8 targeting pack/developer pack** component is selected.
7. Restart Visual Studio and any PowerShell or terminal windows.

## Verification commands

Run these in a new PowerShell window:

```powershell
dotnet new list csiimod

[Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'User')
[Environment]::GetEnvironmentVariable('CSII_MANAGEDPATH', 'User')
[Environment]::GetEnvironmentVariable('CSII_LOCALMODSPATH', 'User')
[Environment]::GetEnvironmentVariable('CSII_USERDATAPATH', 'User')
[Environment]::GetEnvironmentVariable('CSII_MODPOSTPROCESSORPATH', 'User')
```

The template command should list `csiimod`, and each environment-variable check
should return a non-empty path.

## Test-city preparation

1. Create a dedicated local playset for development builds.
2. Create a disposable city rather than using the only copy of a real save.
3. If an existing city is needed for coverage, copy its save first.
4. Include representative EU and NA roads, intersections, roundabouts, bridges,
   elevated roads, tunnels, one-way roads, highways, and official DLC roads.
5. Validate local changes before publishing a new version to Paradox Mods.

The remaining steps are local installation, log inspection, and in-game
validation in a disposable city.

