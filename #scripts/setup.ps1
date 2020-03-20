Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

@('WebAssets', 'AspNetCore.Demo', 'Owin.Demo') | % {
    Write-Host ''
    Write-Host $_ -ForegroundColor White
    Push-Location $_
    try {
        npm install
        if ($LastExitCode -ne 0) {
            throw "npm install exited with code $LastExitCode"
        }
        
        npm run build
        if ($LastExitCode -ne 0) {
            throw "npm run build exited with code $LastExitCode"
        }
    }
    finally {
        Pop-Location
    }
}