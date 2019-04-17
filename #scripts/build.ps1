Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

Write-Output 'npm'
try {
    Write-Output '  WebAssets'
    Push-Location 'WebAssets'
    npm install
    npm run build
    Pop-Location
    
    Write-Output '  Owin.Demo'
    Push-Location 'Owin.Demo'
    npm install
    Pop-Location
    
    Write-Output '  AspNetCore.Demo'
    Push-Location 'AspNetCore.Demo'
    npm install
}
finally {
    Pop-Location
}

Write-Output 'dotnet build'
dotnet build