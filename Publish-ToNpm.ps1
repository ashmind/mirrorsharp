$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

try {
    Set-Content '.npmrc' "//registry.npmjs.org/:_authToken=$($env:NpmAuthToken)"
    Get-Item *.tgz | % { npm publish $($_.FullName) }
}
finally {
    Remove-Item ".npmrc"
}