Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

Write-Output 'dotnet restore'
dotnet restore
if ($LastExitCode -ne 0) {
    throw "dotnet restore exited with code $LastExitCode"
}

Write-Output 'npm install'
@('WebAssets', 'Owin.Demo', 'AspNetCore.Demo') | % {
    try {
        Write-Output "  $_"
        Push-Location $_
        # not worth using npm ci until https://github.com/npm/npm/issues/20104
        npm install --no-audit
        if ($LastExitCode -ne 0) {
            throw "npm ci exited with code $LastExitCode"
        }
    }
    finally {
        Pop-Location
    }
}