Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

dotnet test Tests.Roslyn2.NetCore\Tests.Roslyn2.NetCore.csproj
dotnet test Tests.Roslyn2.Net46\Tests.Roslyn2.Net46.csproj