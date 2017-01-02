@echo off
powershell "$version = 'pre-'+([DateTime]::Now.ToString('yyyyMMdd'))+'%1'; @('MirrorSharp.Common', 'MirrorSharp.Owin') | %% { dotnet pack $_ --version-suffix=$version --output . }"