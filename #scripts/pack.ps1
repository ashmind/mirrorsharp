param (
    [string] $parameter
)

Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

$configuration = 'Debug'
$versionSuffix = $parameter
if ($parameter -match 'Debug|Release') {
    $configuration = $parameter
    $versionSuffix = ''
}

$output = (Resolve-Path .)

@(
  'Common',
  'VisualBasic', 'FSharp', 'Php',
  'Owin', 'AspNetCore',
  'Testing'
) | % {
    dotnet pack $_ --version-suffix=$versionSuffix --output $output --configuration $configuration --no-build --no-restore
    if ($LastExitCode -ne 0) {
        throw "dotnet pack exited with code $LastExitCode"
    }
}

npm pack ./WebAssets/dist
if ($LastExitCode -ne 0) {
    throw "npm pack exited with code $LastExitCode"
}