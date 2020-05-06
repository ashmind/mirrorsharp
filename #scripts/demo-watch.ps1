Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

Wait-Process -InputObject @(
  (Start-Process 'npm' @('run', 'watch') -NoNewWindow -WorkingDirectory "$PSScriptRoot/../WebAssets" -PassThru),
  (Start-Process 'npm' @('run', 'watch') -NoNewWindow -WorkingDirectory "$PSScriptRoot/../AspNetCore.Demo" -PassThru),
  (Start-Process 'dotnet' @('run') -NoNewWindow -WorkingDirectory "$PSScriptRoot/../AspNetCore.Demo" -PassThru)
)

Write-Host "Watch stopped"