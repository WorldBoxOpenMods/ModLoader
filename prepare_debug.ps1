param(
    [string]$GamePath = "C:\Program Files (x86)\Steam\steamapps\common\worldbox",
    [string]$BuildPath = "",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$ProjectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($BuildPath)) {
    $BuildPath = Join-Path $ProjectPath "bin\$Configuration\net48"
}

$BuildPath = (Resolve-Path $BuildPath).Path
if (-not (Test-Path $BuildPath -PathType Container)) {
    $BuildPath = Split-Path -Parent $BuildPath
}

$ModsPath = Join-Path $GamePath "worldbox_Data\StreamingAssets\mods"
New-Item -ItemType Directory -Path $ModsPath -Force | Out-Null

$PrimaryAssembly = Join-Path $BuildPath "NeoModLoader.dll"
if (-not (Test-Path $PrimaryAssembly)) {
    throw "NeoModLoader.dll was not found in $BuildPath"
}

Copy-Item -Path $PrimaryAssembly -Destination (Join-Path $ModsPath "NeoModLoader.dll") -Force
Copy-Item -Path (Join-Path $BuildPath "NeoModLoader.pdb") -Destination (Join-Path $ModsPath "NeoModLoader.pdb") -Force
Copy-Item -Path (Join-Path $BuildPath "NeoModLoader.xml") -Destination (Join-Path $ModsPath "NeoModLoader.xml") -Force

$ConfigPath = Join-Path $BuildPath "NeoModLoader.dll.config"
if (Test-Path $ConfigPath) {
    Copy-Item -Path $ConfigPath -Destination (Join-Path $ModsPath "NeoModLoader.dll.config") -Force
}
