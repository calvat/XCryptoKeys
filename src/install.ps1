# Installe (ou met à jour) le bot sur un serveur Windows : construit l'exécutable, le copie dans
# InstallDir et crée la tâche planifiée qui le lance toutes les IntervalMinutes minutes.
# À lancer dans un PowerShell administrateur, depuis le clone du dépôt :
#   .\src\install.ps1                         # installation ou mise à jour
#   .\src\install.ps1 -WhatIf                 # montre ce qui serait fait, sans rien changer
#   .\src\install.ps1 -NoStart                # tâche créée mais désactivée
# Relancer le script met à jour l'exécutable et la tâche ; appsettings.local.json et les logs sont gardés.
# Désinstallation : .\src\uninstall.ps1
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$InstallDir = "C:\xck-publisher",
    # Vide = le dépôt qui contient ce script (calculé plus bas : $PSScriptRoot est vide ici sous PowerShell 5.1).
    [string]$RepoPath = "",
    [string]$TaskName = "X-Bots Publisher",
    [ValidateRange(1, 1440)][int]$IntervalMinutes = 15,
    [string]$Rid = "win-x64",
    # Compte qui lance la tâche : il doit avoir les identifiants git qui permettent de pousser.
    [string]$User = "$env:USERDOMAIN\$env:USERNAME",
    [switch]$SkipBuild,
    [switch]$NoStart,
    # Pas de demande de confirmation pour les campagnes actives.
    [switch]$Force
)
$ErrorActionPreference = "Stop"
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $RepoPath) { $RepoPath = Split-Path -Parent $scriptDir }

function Fail([string]$message) { Write-Host "ERREUR : $message" -ForegroundColor Red; exit 1 }
function Step([string]$message) { Write-Host ""; Write-Host "== $message" -ForegroundColor Cyan }

# 1. Vérifications
Step "Vérifications"
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin -and -not $WhatIfPreference) { Fail "lancer ce script dans un PowerShell administrateur (clic droit > Exécuter en tant qu'administrateur)." }
if (-not $admin) { Write-Warning "simulation (-WhatIf) sans droits administrateur : l'installation réelle devra être lancée dans un PowerShell administrateur." }
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { Fail "git est introuvable dans le PATH. Installer Git for Windows, puis relancer." }
$RepoPath = (Resolve-Path $RepoPath).Path
if (-not (Test-Path (Join-Path $RepoPath ".git"))) { Fail "$RepoPath n'est pas un clone git du dépôt (option -RepoPath)." }
$accountsDir = Join-Path $RepoPath "accounts"
if (-not (Test-Path $accountsDir -PathType Container)) { Fail "dossier accounts\ introuvable dans $RepoPath." }
$InstallDir = [IO.Path]::GetFullPath($InstallDir)
if ($InstallDir.StartsWith($RepoPath.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
    Fail "InstallDir doit être hors du dépôt ($RepoPath), pour que les git pull ne le touchent pas."
}
Write-Host "Dépôt       : $RepoPath"
Write-Host "Installation: $InstallDir"
Write-Host "Tâche       : « $TaskName », toutes les $IntervalMinutes min, compte $User"

# Clés X : seulement un avertissement, les clés ne sont jamais créées ni affichées ici.
$waiting = @()
$accounts = Get-ChildItem $accountsDir -Directory | Where-Object { $_.Name -notmatch '^[_.]' -and (Test-Path (Join-Path $_.FullName "threads")) }
foreach ($a in $accounts) {
    if ((Test-Path (Join-Path $a.FullName "account.json")) -and -not (Test-Path (Join-Path $a.FullName "account.local.json"))) {
        $waiting += $a.Name
        Write-Host "[$($a.Name)] pas encore de clés X : le bot s'installe quand même et attend, sans rien publier pour ce compte." -ForegroundColor Yellow
        Write-Host "    Plus tard : copier accounts\XCryptoKeys\account.local.example.json en accounts\$($a.Name)\account.local.json et y mettre les 4 clés."
        Write-Host "    Le fichier est relu à chaque passage : la publication commence au passage suivant, sans réinstaller."
    }
}

# 2. Construction
$publishDir = Join-Path $scriptDir "publish\$Rid"
if ($WhatIfPreference -and -not $SkipBuild) {
    Step "Construction ($Rid)"
    Write-Host "WhatIf : construction de l'exécutable avec dotnet publish (non lancée)."
}
elseif (-not $SkipBuild) {
    Step "Construction ($Rid)"
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { Fail "SDK .NET 10 introuvable (dotnet). L'installer, ou construire ailleurs et utiliser -SkipBuild." }
    Push-Location $scriptDir
    try {
        dotnet publish XCryptoKeys.Publisher -c Release -r $Rid --self-contained -p:PublishSingleFile=true -o "publish/$Rid"
        if ($LASTEXITCODE -ne 0) { Fail "dotnet publish a échoué." }
    }
    finally { Pop-Location }
}
$exeSource = Join-Path $publishDir "xck-publisher.exe"
if (-not $WhatIfPreference -and -not (Test-Path $exeSource)) { Fail "exécutable introuvable : $exeSource (lancer sans -SkipBuild)." }

# 3. Copie (appsettings.local.json et logs\ ne sont jamais écrasés)
Step "Copie dans $InstallDir"
$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($task -and $task.State -eq "Running" -and $PSCmdlet.ShouldProcess($TaskName, "Arrêter le passage en cours")) {
    Stop-ScheduledTask -TaskName $TaskName
}
if ($PSCmdlet.ShouldProcess($InstallDir, "Copier l'exécutable et appsettings.json")) {
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    Get-ChildItem $publishDir -File | Where-Object { $_.Name -ne "appsettings.local.json" } |
        Copy-Item -Destination $InstallDir -Force
}
$localSettings = Join-Path $InstallDir "appsettings.local.json"
if (Test-Path $localSettings) {
    Write-Host "appsettings.local.json existe déjà : gardé tel quel."
}
elseif ($PSCmdlet.ShouldProcess($localSettings, "Créer avec RepoPath = $RepoPath")) {
    $json = @{ RepoPath = $RepoPath } | ConvertTo-Json
    [IO.File]::WriteAllText($localSettings, $json + "`r`n", (New-Object Text.UTF8Encoding $false))
    Write-Host "appsettings.local.json créé (RepoPath = $RepoPath)."
}

# 4. Test à blanc
$exe = Join-Path $InstallDir "xck-publisher.exe"
if (Test-Path $exe) {
    Step "Test à blanc (--dry-run : rien n'est publié ni modifié)"
    & $exe --dry-run
    if ($LASTEXITCODE -ne 0) { Write-Warning "le dry-run s'est terminé avec le code $LASTEXITCODE : lire les messages ci-dessus avant d'activer la tâche." }
}

# 5. Campagnes actives : leur prochain post partira au premier vrai passage
$active = @()
foreach ($a in $accounts) {
    foreach ($s in Get-ChildItem (Join-Path $a.FullName "threads") -Directory | Where-Object { $_.Name -notmatch '^_' }) {
        $statusFile = Join-Path $s.FullName "status.json"
        if (-not (Test-Path $statusFile)) { continue }
        $status = Get-Content $statusFile -Raw -Encoding UTF8 | ConvertFrom-Json
        if ($status.status -eq "active") {
            $active += "  $($a.Name)/$($s.Name) : post #$([int]$status.last_posted_index + 1), à $($status.post_time) UTC"
        }
    }
}
$enable = -not $NoStart
if ($enable -and $active.Count -gt 0) {
    Step "Campagnes actives"
    Write-Host "Ces campagnes publieront leur prochain post au premier passage après l'heure indiquée (aujourd'hui si elle est passée) :"
    $active | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
    if (-not $Force -and -not $WhatIfPreference) {
        $answer = Read-Host "Activer la tâche maintenant ? (o/N)"
        if ($answer -notmatch '^(o|oui|y|yes)$') {
            $enable = $false
            Write-Host "La tâche sera créée désactivée. Pour l'activer plus tard : Enable-ScheduledTask -TaskName '$TaskName'"
        }
    }
}

# 6. Tâche planifiée
Step "Tâche planifiée « $TaskName »"
$action = New-ScheduledTaskAction -Execute $exe -WorkingDirectory $InstallDir
# Répétition sans fin (pas de RepetitionDuration), à partir de la minute suivante.
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes $IntervalMinutes)
$settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -StartWhenAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Minutes 10) -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
if ($PSCmdlet.ShouldProcess($TaskName, "Créer ou mettre à jour la tâche (compte $User)")) {
    # La tâche tourne que l'utilisateur soit connecté ou non : Windows demande son mot de passe.
    # Il n'est gardé qu'en mémoire le temps de l'enregistrement, jamais écrit dans un fichier.
    $cred = Get-Credential -UserName $User -Message "Mot de passe de $User, pour lancer la tâche même sans session ouverte"
    if (-not $cred) { Fail "pas de mot de passe : tâche non créée." }
    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings `
        -User $cred.UserName -Password $cred.GetNetworkCredential().Password -RunLevel Limited `
        -Description "Publie sur X les posts dus des comptes de $accountsDir (voir src\README.md)." -Force | Out-Null
    if ($enable) { Enable-ScheduledTask -TaskName $TaskName | Out-Null; Write-Host "Tâche active : premier passage dans une minute." -ForegroundColor Green }
    else { Disable-ScheduledTask -TaskName $TaskName | Out-Null; Write-Host "Tâche créée, désactivée." -ForegroundColor Yellow }
}

Write-Host ""
if ($waiting.Count -gt 0 -and -not $WhatIfPreference) {
    Write-Host "Installé. En attente des clés X pour : $($waiting -join ', '). Rien ne sera publié pour ces comptes tant que leur account.local.json n'est pas rempli." -ForegroundColor Yellow
}
Write-Host "Logs : $InstallDir\logs\publisher-AAAA-MM-JJ.log"
Write-Host "Lancer un passage tout de suite : Start-ScheduledTask -TaskName '$TaskName'"
