# Building and reviewing version 1.0

## Build environment

- Windows x64.
- .NET 10 SDK (original build: SDK 10.0.401).
- NuGet access for Microsoft.Web.WebView2 1.0.4258.31 (pinned in the project).
- Node.js for the JavaScript tests only; no npm packages are required.

## Restore, build and test

Run in the repository root:

```powershell
dotnet restore Desktop/PermafrostLiveMap.csproj
dotnet build Desktop/PermafrostLiveMap.csproj -c Release --no-restore
dotnet run --project Tests/Tests.csproj -c Release
node --check Companion/dist/app.js
node motion.test.mjs
```

## Publish

```powershell
dotnet publish Desktop/PermafrostLiveMap.csproj -c Release --self-contained false -o artifacts/N0M4DLiveMap -p:DebugType=None -p:DebugSymbols=false
```

Or run `./Build-Package.ps1` to create the app/mod layout and a ZIP under artifacts. The script intentionally fails if its staging folder already exists so it cannot accidentally package stale files. Move that folder aside before rebuilding.

The output is framework-dependent: install .NET 10 Windows Desktop Runtime x64, ASP.NET Core Runtime 10 x64, and Microsoft Edge WebView2 Runtime before running the app. The SDK is not needed by end users. UE4SS must be installed separately in a version compatible with the game.

The package script removes WebView2 XML documentation, x86/ARM64 loaders and the browser's development package.json. WebView2's managed DLLs and x64 native loader are retained. The generated dependency manifest references the WinForms assembly supplied by the WebView2 package, so that assembly is retained too.

## Source-to-package correspondence

This source contains the app, telemetry adapter, map data and assets used for the cleaned 1.0 package. The original uploaded clean ZIP SHA256 was:

`6A75070DDC1564332570EC9D61C376B0DF22535882BAD07FCAE2889D922BFB68`

A rebuild is not promised to be byte-identical: SDK/environment differences and ZIP timestamps may change hashes. No obfuscator, executable packer or installer is used.

For installed operation, put N0M4DLiveMap beside Permafrost.exe and the adapter under Permafrost/Binaries/Win64/ue4ss/Mods/N0M4DTelemetry. Full install and rollback instructions are in INSTALL.txt.

## Safety-review context

The Nexus quarantine message lists possible reasons but does not identify a specific detection. Build/test success does not establish malware-scan status. Review the source and dependencies and compare the supplied ZIP independently.
