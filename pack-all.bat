@echo off
powershell "$version = 'pre-'+([DateTime]::Now.ToString('yyyyMMdd'))+'%1'; @('Common', 'Owin', 'Testing') | %% { dotnet pack $_ --version-suffix=$version --output . --configuration Release }"
npm pack WebAssets