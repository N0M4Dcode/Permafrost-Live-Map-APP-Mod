# N0M4D's Permafrost Live Map

Source for version 1.0 of the Windows desktop adaptation of azam8's Permafrost Live Map Companion. Prepared for a private repository and Nexus Mods source review.

Nexus page: https://www.nexusmods.com/permafrost/mods/28
Original companion by azam8: https://www.nexusmods.com/permafrost/mods/25
UE4SS GitHub: https://github.com/UE4SS-RE/RE-UE4SS
UE4SS releases: https://github.com/UE4SS-RE/RE-UE4SS/releases

## Contents

- `Desktop/`: .NET 10 WPF host, custom window controls, native HTTP server and application icon.
- `Companion/dist/`: retained/adapted map interface, data and motion interpolation.
- `N0M4DTelemetry/`: separate read-only UE4SS Lua adapter.
- `Tests/`: native feed tests.
- `motion.test.mjs`: JavaScript interpolation tests.
- `BUILD.md`: dependencies and exact build/review steps.
- `INSTALL.txt`: end-user requirements and installation instructions.
- `CREDITS.md`: original authors, component provenance and permission status.

## Build

On Windows x64 with the .NET 10 SDK installed:

```powershell
dotnet restore Desktop/PermafrostLiveMap.csproj
dotnet build Desktop/PermafrostLiveMap.csproj -c Release
```

Run `./Build-Package.ps1` from PowerShell to build a local distribution ZIP. This is a developer build script; users launch the resulting EXE directly without scripts or Python. See BUILD.md for tests and manual steps.

## Behaviour and review scope

The Lua adapter reads the local pawn's position/heading on the game thread and writes `position.json` inside its mod folder. It does not change saves, inventory or player input. The desktop host serves its interface and telemetry at `127.0.0.1:8767`. It reads the mod path relative to its installed directory, with an optional explicit feed-file path as its first command-line argument.

External map tile requests go to MetaBot.GG. Reference links open in the user's browser. WebView2 uses a persistent profile under `%LOCALAPPDATA%/N0M4D/PermafrostLiveMap/WebView2` for preferences, pins and calibration. The repository does not contain the user's live telemetry or profile.

## Validation status

Release compilation, JavaScript syntax, motion tests, feed tests, local HTTP checks and UE4SS adapter startup were checked. Full gameplay smoothness, measured 60 FPS and complete interactive window/control regression remain unverified. Nexus quarantined the uploaded binary package; the exact detection or trigger has not been provided. These functional checks are not a malware clearance.

## Attribution and permissions

The map interface retains/adapts azam8's work. Map tiles and reference data are credited to MetaBot.GG; Permafrost artwork belongs to the game's rights holders. See CREDITS.md. No blanket open-source license is applied to these third-party components. Keeping this repository private does not grant redistribution permission. Obtain applicable author permissions before publication.

A private repository link alone will not give Nexus reviewers access. Arrange reviewer access with Nexus support; do not put tokens or account credentials in the repository.
