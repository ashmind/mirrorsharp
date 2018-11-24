Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

$errorExitCode = 0
@('NetCore', 'Net46') | % {
    dotnet test "Tests.Roslyn2.$_\Tests.Roslyn2.$_.csproj"
    if ($LastExitCode -ne 0) {
        $errorExitCode = $LastExitCode
    }
}

if ($errorExitCode -ne 0) {
    throw "One of dotnet test commands exited with exit code $errorExitCode"
}