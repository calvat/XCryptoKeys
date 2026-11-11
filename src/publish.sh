#!/usr/bin/env sh
# Construit l'exécutable autonome (pas besoin d'installer .NET sur le serveur).
# Usage : ./publish.sh [linux-x64|linux-arm64|win-x64]   (défaut : linux-x64)
set -e
RID="${1:-linux-x64}"
cd "$(dirname "$0")"
dotnet publish XCryptoKeys.Publisher -c Release -r "$RID" --self-contained -p:PublishSingleFile=true -o "publish/$RID"
echo "Exécutable dans src/publish/$RID/"
