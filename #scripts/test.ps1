Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

$errorExitCode = 0
@('NetCore', 'Net46', 'RoslynLatest') | % {
    dotnet test "Tests.$_\Tests.$_.csproj"
    if ($LastExitCode -ne 0) {
        $errorExitCode = $LastExitCode
    }
}

if ($errorExitCode -ne 0) {
    throw "One of dotnet test commands exited with exit code $errorExitCode"
}