@echo off
powershell "$version = 'pre-'+([DateTime]::Now.ToString('yyyyMMdd'))+'%1'; $output = (Resolve-Path .); @('Common', 'FSharp', 'Owin', 'Testing') | %% { dotnet restore /p:VersionSuffix=$version; dotnet pack $_ --version-suffix=$version --output $output --configuration Release }"
npm pack WebAssets