# Arrête et supprime le service Windows du bot (et l'ancienne tâche planifiée s'il y en a une).
# Avec -RemoveFiles, supprime aussi le dossier d'installation (exécutable, appsettings.local.json, logs), sauf la clé SSH
# de déploiement (dossier ssh\), supprimée seulement avec -RemoveDeployKey. Ne touche jamais au dépôt ni aux account.local.json.
# À lancer dans un PowerShell administrateur :
#   .\src\uninstall.cmd
#   .\src\uninstall.cmd -RemoveFiles
#   .\src\uninstall.cmd -RemoveFiles -RemoveDeployKey
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = "High")]
param(
    [string]$InstallDir = "C:\xck-publisher",
    [string]$ServiceName = "XBotsPublisher",
    [string]$TaskName = "X-Bots Publisher",
    [switch]$RemoveFiles,
    [switch]$RemoveDeployKey
)
$ErrorActionPreference = "Stop"

$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin -and -not $WhatIfPreference) { Write-Host "ERREUR : lancer ce script dans un PowerShell administrateur." -ForegroundColor Red; exit 1 }

$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($svc) {
    if ($PSCmdlet.ShouldProcess($ServiceName, "Arrêter et supprimer le service")) {
        if ($svc.Status -ne "Stopped") { Stop-Service -Name $ServiceName -Force }
        & sc.exe delete $ServiceName | Out-Null
        Write-Host "Service « $ServiceName » supprimé."
    }
}
else {
    Write-Host "Pas de service « $ServiceName »."
}

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
    $sshDir = Join-Path $InstallDir "ssh"
    if ($RemoveDeployKey) {
        if ($PSCmdlet.ShouldProcess($InstallDir, "Supprimer le dossier, clé de déploiement comprise")) {
            Remove-Item -Recurse -Force $InstallDir
            Write-Host "Dossier $InstallDir supprimé, clé de déploiement comprise."
            Write-Host "Penser à supprimer aussi la clé sur GitHub : dépôt > Settings > Deploy keys (« x-bots service $env:COMPUTERNAME »)." -ForegroundColor Yellow
        }
    }
    elseif ($PSCmdlet.ShouldProcess($InstallDir, "Supprimer l'exécutable, appsettings.local.json et les logs (la clé ssh\ est gardée)")) {
        Get-ChildItem $InstallDir -Force | Where-Object { $_.FullName -ne $sshDir } | Remove-Item -Recurse -Force
        Write-Host "Dossier $InstallDir vidé ; la clé de déploiement (ssh\) est gardée (-RemoveDeployKey pour la supprimer)."
    }
}
