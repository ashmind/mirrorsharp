Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

Write-Output 'dotnet test'
dotnet test
if ($LastExitCode -ne 0) {
    throw "dotnet test exited with code $LastExitCode"
}

Write-Output 'npm test'
try {
    Push-Location 'WebAssets'
    npm test
    if ($LastExitCode -ne 0) {
        throw "npm test exited with code $LastExitCode"
    }
}
finally {
    Pop-Location
}