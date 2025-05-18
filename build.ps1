<#
.SYNOPSIS
    Bite Build Engine Helper Script in PowerShell
.DESCRIPTION
    Wrapper for dotnet CLI commands.
    Also detects solution file, manages SDK per global.json (install locally if missing).
#>
param(
    [Parameter(Position=0, Mandatory=$true)]
    [ValidateSet('restore','clean','build','test','bite','help')]
    [string]$Action,
    [Parameter(ValueFromRemainingArguments=$true)]
    [string[]]$Arguments
)

function Show-Help {
	Write-Host @"
Usage: $($MyInvocation.MyCommand.Name) <command> [additional dotnet args]

Commands:
  build               Build the solution (default)
  test                Run unit tests in the solution
  restore             Restore NuGet packages for the solution
  clean               Clean outputs for the solution
  bite [target]       Invoke bite target
  help                Show this message and exit

Example:
  .\$($MyInvocation.MyCommand.Name) build -c Release
"@ -ForegroundColor Cyan
    exit 0
}

function Install-SDK {
    param($Version)
    $dir = Join-Path $PSScriptRoot '.dotnet'
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
    $installer = Join-Path $dir 'dotnet-install.ps1'
    Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer
    & $installer -Version $Version -InstallDir $dir
    if ($LASTEXITCODE -ne 0) { throw "SDK install failed ($LASTEXITCODE)" }
    $env:DOTNET_ROOT = $dir; $env:PATH = "$dir;$env:PATH"
    return Join-Path $dir 'dotnet'
}

function Resolve-DotNet {
    $json = Join-Path $PSScriptRoot 'global.json'
    $required = if (Test-Path $json) { (Get-Content $json|ConvertFrom-Json).sdk.version } else { 'latest' }
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        if ($required -ne 'latest') {
            $sdks = dotnet --list-sdks | ForEach-Object { $_.Split(' ')[0] }
            if ($sdks -notcontains $required) { return Install-SDK $required }
        }
        return 'dotnet'
    }
    return Install-SDK $required
}

function Find-Solution {
    $s = Get-ChildItem -Path $PSScriptRoot -Filter '*.sln'
    if ($s.Count -eq 1) { return $s[0].FullName }
    elseif ($s.Count -eq 0) { throw 'No .sln found' }
    else { throw "Multiple .sln found: $($s.Name -join ', ')" }
}

# CI-friendly environment
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_UI_LANGUAGE = 'en'
$env:MSBUILDTERMINALLOGGER = 'off'

# Shared default args
$DefaultArgs = @('--nologo')

if ($Action -eq 'help') { Show-Help }

$dotnet = Resolve-DotNet
$solution = Find-Solution

switch ($Action) {
    'restore' { $cmd = @($dotnet,'restore') + $DefaultArgs + @($solution) + $Arguments }
    'clean'   { $cmd = @($dotnet,'clean')   + $DefaultArgs + @($solution) + $Arguments }
    'build'   { $cmd = @($dotnet,'build')   + $DefaultArgs + @($solution) + $Arguments }
    'test'   { $cmd = @($dotnet,'test')   + $DefaultArgs + @($solution) + $Arguments }
    'bite'    {
        if ($Arguments.Count -gt 0 -and $Arguments[0] -notmatch '^[\-/]') {
            $t = $Arguments[0]
			if ($Arguments.Count -gt 1) {
				$Arguments = $Arguments[1..($Arguments.Count-1)]
			} else { $Arguments = $() }
        } else { $t='help' }
        $cmd = @($dotnet,'msbuild') + $DefaultArgs + @("-t:$t",'bite.proj') + $Arguments
    }
}

Write-Host "Executing: $($cmd -join ' ')"
& $cmd[0] $cmd[1..($cmd.Length-1)]
