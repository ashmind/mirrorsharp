param (
    [string] $versionSuffix = ''
)

Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

$output = (Resolve-Path .)

@(
  'Common',
  'VisualBasic', 'FSharp', 'Php',
  'Owin', 'AspNetCore',
  'Testing'
) | % {
    dotnet pack $_ --version-suffix=$versionSuffix --output $output --configuration Release --no-build --no-restore
    if ($LastExitCode -ne 0) {
        throw "dotnet pack exited with code $LastExitCode"
    }
}

npm pack ./WebAssets/dist
if ($LastExitCode -ne 0) {
    throw "npm pack exited with code $LastExitCode"
}