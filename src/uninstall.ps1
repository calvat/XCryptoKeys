# Supprime la tâche planifiée du bot. Avec -RemoveFiles, supprime aussi le dossier d'installation
# (exécutable, appsettings.local.json, logs). Ne touche jamais au dépôt ni aux account.local.json.
# À lancer dans un PowerShell administrateur :
#   .\src\uninstall.ps1
#   .\src\uninstall.ps1 -RemoveFiles
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = "High")]
param(
    [string]$InstallDir = "C:\xck-publisher",
    [string]$TaskName = "X-Bots Publisher",
    [switch]$RemoveFiles
)
$ErrorActionPreference = "Stop"

$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin -and -not $WhatIfPreference) { Write-Host "ERREUR : lancer ce script dans un PowerShell administrateur." -ForegroundColor Red; exit 1 }

$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($task) {
    if ($PSCmdlet.ShouldProcess($TaskName, "Supprimer la tâche planifiée")) {
        if ($task.State -eq "Running") { Stop-ScheduledTask -TaskName $TaskName }
        Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
        Write-Host "Tâche « $TaskName » supprimée."
    }
}
else {
    Write-Host "Pas de tâche « $TaskName »."
}

if ($RemoveFiles) {
    $InstallDir = [IO.Path]::GetFullPath($InstallDir)
    if (-not (Test-Path $InstallDir)) { Write-Host "Dossier $InstallDir absent."; exit 0 }
    # Garde-fou : ne jamais supprimer un clone du dépôt ou un dossier qui n'est pas une installation du bot.
    if ((Test-Path (Join-Path $InstallDir ".git")) -or (Test-Path (Join-Path $InstallDir "accounts")) -or
        -not (Test-Path (Join-Path $InstallDir "xck-publisher.exe"))) {
        Write-Host "ERREUR : $InstallDir ne ressemble pas à un dossier d'installation du bot ; rien n'est supprimé." -ForegroundColor Red
        exit 1
    }
    if ($PSCmdlet.ShouldProcess($InstallDir, "Supprimer le dossier (exécutable, appsettings.local.json, logs)")) {
        Remove-Item -Recurse -Force $InstallDir
        Write-Host "Dossier $InstallDir supprimé."
    }
}
