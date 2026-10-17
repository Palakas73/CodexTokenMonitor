$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_CLI_HOME = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/dotnet-home'
$env:NUGET_PACKAGES = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/nuget-packages'
$taskDotnet = Join-Path $env:ProgramFiles 'dotnet/dotnet.exe'
if (!(Test-Path -LiteralPath $taskDotnet)) { throw '.NET 10 x64 SDK is required.' }
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    & $taskDotnet test CodexTokenMonitor.slnx -c Release --logger 'trx;LogFileName=publish.trx' --results-directory artifacts/TestResults
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    & $taskDotnet publish src/CodexTokenMonitor.App/CodexTokenMonitor.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -p:DebugType=None -p:DebugSymbols=false -o artifacts/win-x64
    if ($LASTEXITCODE -ne 0) { throw 'Publishing failed.' }
    $taskVersion = ([xml](Get-Content -LiteralPath 'Directory.Build.props' -Raw)).Project.PropertyGroup.Version
    $taskZip = "artifacts/CodexTokenMonitor-v$taskVersion-win-x64.zip"
    Compress-Archive -LiteralPath 'artifacts/win-x64/CodexTokenMonitor.App.exe', 'README.md', "docs/release-notes-v$taskVersion.md" -DestinationPath $taskZip -Force
    Write-Output "Package created: $taskZip"
} finally { Pop-Location }
