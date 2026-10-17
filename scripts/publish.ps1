$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_CLI_HOME = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/dotnet-home'
$env:NUGET_PACKAGES = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/nuget-packages'
$taskDotnet = Join-Path $env:ProgramFiles 'dotnet/dotnet.exe'
if (!(Test-Path -LiteralPath $taskDotnet)) { throw 'Se requiere SDK .NET 10 x64.' }
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    & $taskDotnet test CodexTokenMonitor.slnx -c Release --logger 'trx;LogFileName=publish.trx' --results-directory artifacts/TestResults
    if ($LASTEXITCODE -ne 0) { throw 'Las pruebas han fallado.' }
    & $taskDotnet publish src/CodexTokenMonitor.App/CodexTokenMonitor.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -p:DebugType=None -p:DebugSymbols=false -o artifacts/win-x64
    if ($LASTEXITCODE -ne 0) { throw 'La publicación ha fallado.' }
    $taskVersion = ([xml](Get-Content -LiteralPath 'Directory.Build.props' -Raw)).Project.PropertyGroup.Version
    $taskZip = "artifacts/CodexTokenMonitor-v$taskVersion-win-x64.zip"
    Compress-Archive -LiteralPath 'artifacts/win-x64/CodexTokenMonitor.App.exe', 'README.md', "docs/release-notes-v$taskVersion.md" -DestinationPath $taskZip -Force
    Write-Output "Paquete generado: $taskZip"
} finally { Pop-Location }
