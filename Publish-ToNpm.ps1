$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

try {
    Set-Content '.npmrc' "//registry.npmjs.org/:_authToken=$($env:NpmAuthToken)"
    Get-Item *.tgz | % { 
        try {
            npm publish $($_.FullName)
        }
        catch {
            # not failing since it's likely we tried to publish package
            # twice with the same name. later this should be improved
            # to actually check the error.
            Write-Output $_
        }
    }
}
finally {
    Remove-Item ".npmrc"
}