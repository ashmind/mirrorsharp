@echo off
powershell "$version = 'pre-'+([DateTime]::Now.ToString('yyyyMMdd'))+'%1'; @('MirrorSharp.Common', 'MirrorSharp.Owin', 'MirrorSharp.Testing') | %% { dotnet pack $_ --version-suffix=$version --output . --configuration Release }"
npm pack MirrorSharp.WebAssets