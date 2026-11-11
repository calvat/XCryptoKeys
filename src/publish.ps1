# Construit l'exécutable autonome (pas besoin d'installer .NET sur le serveur).
# Usage : .\publish.ps1 [-Rid win-x64|linux-x64|linux-arm64]   (défaut : win-x64)
param([string]$Rid = "win-x64")
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
dotnet publish XCryptoKeys.Publisher -c Release -r $Rid --self-contained -p:PublishSingleFile=true -o "publish/$Rid"
Write-Host "Exécutable dans src/publish/$Rid/"
