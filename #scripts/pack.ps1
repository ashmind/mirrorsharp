param (
    [string] $versionSuffix = ''
)

Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

function Invoke-External($command) {
    Write-Host
    Write-Host $command -ForegroundColor White

    Invoke-Expression $command
    if ($LastExitCode -ne 0) {
        throw "Command finished with exit code $LastExitCode"
    }
}

$output = (Resolve-Path .)

@('Common', 'FSharp', 'Owin', 'Testing', 'VisualBasic', 'Php') | % {
    Invoke-External "dotnet pack $_ --version-suffix=$versionSuffix --output '$output' --configuration Release"
}

Push-Location WebAssets
try {
    Invoke-External "npm install"
    Invoke-External "npm run build"
}
finally {
    Pop-Location
}

Invoke-External "npm pack ./WebAssets/dist"