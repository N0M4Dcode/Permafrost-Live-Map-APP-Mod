$ErrorActionPreference='Stop'
Set-Location $PSScriptRoot
$stage=Join-Path $PSScriptRoot 'artifacts\stage'
if(Test-Path -LiteralPath $stage){throw 'artifacts/stage already exists. Move it aside before rebuilding.'}
New-Item -ItemType Directory -Path $stage -Force | Out-Null
$app=Join-Path $stage 'N0M4DLiveMap'
dotnet publish Desktop/PermafrostLiveMap.csproj -c Release --self-contained false -o $app -p:DebugType=None -p:DebugSymbols=false
if($LASTEXITCODE -ne 0){throw 'Publish failed'}
# Delete only the named optional outputs inside this new staging directory.
$stageRoot=[IO.Path]::GetFullPath($stage)+[IO.Path]::DirectorySeparatorChar
$optional=@('Microsoft.Web.WebView2.Core.xml','Microsoft.Web.WebView2.Wpf.xml','Microsoft.Web.WebView2.WinForms.xml','dist\package.json','runtimes\win-arm64','runtimes\win-x86')
foreach($relative in $optional){
 $candidate=[IO.Path]::GetFullPath((Join-Path $app $relative))
 if(-not $candidate.StartsWith($stageRoot,[StringComparison]::OrdinalIgnoreCase)){throw 'Invalid staging path'}
 if(Test-Path -LiteralPath $candidate){Remove-Item -LiteralPath $candidate -Recurse -Force}
}
$mods=Join-Path $stage 'Permafrost\Binaries\Win64\ue4ss\Mods'
New-Item -ItemType Directory -Path $mods -Force | Out-Null
Copy-Item -LiteralPath 'N0M4DTelemetry' -Destination $mods -Recurse
Copy-Item -LiteralPath 'INSTALL.txt' -Destination (Join-Path $stage 'README.txt')
Copy-Item -LiteralPath 'CREDITS.md' -Destination $stage
$zip=Join-Path $PSScriptRoot 'artifacts\N0M4D-Permafrost-Live-Map-1.0.zip'
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -Force
Write-Output "Package created: $zip"
