@echo off
powershell "$versionSuffix = '%1'; $output = (Resolve-Path .); @('Common', 'FSharp', 'Owin', 'Testing', 'VisualBasic') | %% { dotnet restore /p:VersionSuffix=$versionSuffix; dotnet pack $_ --version-suffix=$version --output $output --configuration Release }"
npm pack WebAssets