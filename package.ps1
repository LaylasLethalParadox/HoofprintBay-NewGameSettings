# Builds the plugin and makes the release zip: dist\NewGameSettings-<version>.zip
# The zip holds BepInEx\plugins\NewGameSettings\NewGameSettings.dll, so it can be extracted straight into the game folder.
# NoInstall: building the release zip never touches a game install.
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

dotnet build -c Release -p:NoInstall=true
if ($LASTEXITCODE -ne 0) { throw 'The build failed.' }

$version = ([xml](Get-Content NewGameSettings.csproj)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
$dll = Join-Path $PSScriptRoot 'bin\Release\net6.0\NewGameSettings.dll'
New-Item -ItemType Directory -Force dist | Out-Null
$zipPath = Join-Path $PSScriptRoot "dist\NewGameSettings-$version.zip"

Add-Type -AssemblyName System.IO.Compression.FileSystem
$stream = [IO.File]::Open($zipPath, [IO.FileMode]::Create)
$zip = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create)
[void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $dll, 'BepInEx/plugins/NewGameSettings/NewGameSettings.dll')
$zip.Dispose()
$stream.Dispose()
"Made $zipPath"
