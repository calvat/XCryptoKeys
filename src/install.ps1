# Installe (ou met à jour) le bot sur un PC Windows : construit l'exécutable, le copie dans InstallDir et le fait
# tourner en continu, par défaut comme service Windows « XBotsPublisher » (compte LocalService, sans mot de passe,
# démarrage automatique), qui fait un passage toutes les IntervalMinutes minutes.
# À lancer dans un PowerShell administrateur, depuis le clone du dépôt :
#   .\src\install.cmd                          # installation ou mise à jour (service Windows)
#   .\src\install.cmd -WhatIf                  # montre ce qui serait fait, sans rien changer
#   .\src\install.cmd -NoStart                 # service installé mais arrêté (démarrage manuel)
#   .\src\install.cmd -ScheduledTask           # ancien mode : tâche planifiée sous le compte de l'utilisateur
# Relancer le script met à jour l'exécutable et le service ; appsettings.local.json, les logs et la clé SSH sont gardés.
# Désinstallation : .\src\uninstall.cmd
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$InstallDir = "C:\xck-publisher",
    # Vide = le dépôt qui contient ce script (calculé plus bas : $PSScriptRoot est vide ici sous PowerShell 5.1).
    [string]$RepoPath = "",
    [string]$ServiceName = "XBotsPublisher",
    [string]$TaskName = "X-Bots Publisher",
    [ValidateRange(1, 1440)][int]$IntervalMinutes = 15,
    [string]$Rid = "win-x64",
    [switch]$SkipBuild,
    [switch]$NoStart,
    # Pas de demande de confirmation pour les campagnes actives.
    [switch]$Force,
    # Ancien mode : tâche planifiée sous le compte de l'utilisateur (au lieu du service).
    [switch]$ScheduledTask,
    # Mode -ScheduledTask : compte qui lance la tâche (il doit avoir les identifiants git qui permettent de pousser).
    [string]$User = "$env:USERDOMAIN\$env:USERNAME",
    # Mode -ScheduledTask : la tâche ne tourne que pendant que la session est ouverte (pas de mot de passe).
    [switch]$RunOnlyWhenLoggedOn
)
$ErrorActionPreference = "Stop"
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $RepoPath) { $RepoPath = Split-Path -Parent $scriptDir }
$serviceMode = -not $ScheduledTask

function Fail([string]$message) { Write-Host "ERREUR : $message" -ForegroundColor Red; exit 1 }
function Step([string]$message) { Write-Host ""; Write-Host "== $message" -ForegroundColor Cyan }
# SID connus : noms indépendants de la langue de Windows (« SERVICE LOCAL » en français…).
$SidLocalService = "*S-1-5-19"
$SidSystem = "*S-1-5-18"
$SidAdmins = "*S-1-5-32-544"

function Invoke-Native([string]$exe, [string[]]$arguments, [string]$what) {
    # Sortie affichée seulement en cas d'échec (icacls écrit une ligne par fichier traité).
    $output = & $exe @arguments 2>&1
    if ($LASTEXITCODE -ne 0) { $output | ForEach-Object { Write-Host "  $_" }; Fail "$what a échoué (code $LASTEXITCODE)." }
}

# Ajoute ou remplace une valeur « A.B » dans un objet venu de ConvertFrom-Json.
function Set-JsonValue($obj, [string]$path, $value) {
    $parts = $path.Split('.')
    $cur = $obj
    for ($i = 0; $i -lt $parts.Length - 1; $i++) {
        $p = $parts[$i]
        if (-not ($cur.PSObject.Properties.Name -contains $p) -or $null -eq $cur.$p) {
            $cur | Add-Member -NotePropertyName $p -NotePropertyValue ([pscustomobject]@{}) -Force
        }
        $cur = $cur.$p
    }
    $last = $parts[-1]
    $cur | Add-Member -NotePropertyName $last -NotePropertyValue $value -Force
}

# Chargé d'avance : sous -WhatIf, le chargement automatique affiche une ligne par alias.
& { $WhatIfPreference = $false; Import-Module CimCmdlets, ScheduledTasks -ErrorAction SilentlyContinue }

# 1. Vérifications
Step "Vérifications"
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin -and -not $WhatIfPreference) { Fail "lancer ce script dans un PowerShell administrateur (clic droit > Exécuter en tant qu'administrateur)." }
if (-not $admin) { Write-Warning "simulation (-WhatIf) sans droits administrateur : l'installation réelle devra être lancée dans un PowerShell administrateur." }
$gitCmd = Get-Command git -ErrorAction SilentlyContinue
if (-not $gitCmd) { Fail "git est introuvable dans le PATH. Installer Git for Windows, puis relancer." }
$RepoPath = (Resolve-Path $RepoPath).Path
if (-not (Test-Path (Join-Path $RepoPath ".git"))) { Fail "$RepoPath n'est pas un clone git du dépôt (option -RepoPath)." }
$accountsDir = Join-Path $RepoPath "accounts"
if (-not (Test-Path $accountsDir -PathType Container)) { Fail "dossier accounts\ introuvable dans $RepoPath." }
$InstallDir = [IO.Path]::GetFullPath($InstallDir)
if ($InstallDir.StartsWith($RepoPath.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
    Fail "InstallDir doit être hors du dépôt ($RepoPath), pour que les git pull ne le touchent pas."
}
$branch = (& git -C $RepoPath rev-parse --abbrev-ref HEAD).Trim()

if ($serviceMode) {
    # LocalService ne voit pas les lecteurs réseau : le clone doit être sur un disque local.
    if ($RepoPath.StartsWith('\\')) { Fail "le dépôt est sur un chemin réseau ($RepoPath) : le service (LocalService) ne peut pas y accéder. Cloner le dépôt sur un disque local." }
    $driveId = $RepoPath.Substring(0, 2)
    $disk = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='$driveId'" -ErrorAction SilentlyContinue
    if ($disk -and $disk.DriveType -notin 2, 3) { Fail "le dépôt est sur $driveId, qui n'est pas un disque local (lecteur réseau ?) : le service (LocalService) ne peut pas y accéder. Cloner le dépôt sur un disque local, ou utiliser -ScheduledTask." }
    # Adresse SSH du dépôt GitHub, déduite du remote du clone (le clone de l'utilisateur n'est pas modifié).
    $originUrl = (& git -C $RepoPath remote get-url origin).Trim()
    if ($originUrl -match '^(?:https://github\.com/|git@github\.com:|ssh://git@github\.com/)([^/]+)/(.+?)(?:\.git)?/?$') {
        $ghOwner = $Matches[1]; $ghRepo = $Matches[2]
        $sshUrl = "git@github.com:$ghOwner/$ghRepo.git"
    }
    else { Fail "le remote origin ($originUrl) n'est pas un dépôt GitHub : la clé de déploiement du service ne peut pas être configurée. Utiliser -ScheduledTask." }
}

Write-Host "Dépôt       : $RepoPath (branche $branch)"
Write-Host "Installation: $InstallDir"
if ($serviceMode) { Write-Host "Mode        : service Windows « $ServiceName », compte LocalService, un passage toutes les $IntervalMinutes min ; git par SSH ($sshUrl)" }
else { Write-Host "Mode        : tâche planifiée « $TaskName », toutes les $IntervalMinutes min, compte $User" }

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

# 3. Arrêt de ce qui tourne déjà (service ou tâche), puis copie
Step "Copie dans $InstallDir"
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($svc -and $svc.Status -ne "Stopped" -and $PSCmdlet.ShouldProcess($ServiceName, "Arrêter le service")) {
    Stop-Service -Name $ServiceName -Force
}
if ($task -and $task.State -eq "Running" -and $PSCmdlet.ShouldProcess($TaskName, "Arrêter le passage en cours")) {
    Stop-ScheduledTask -TaskName $TaskName
}
if ($PSCmdlet.ShouldProcess($InstallDir, "Copier l'exécutable et appsettings.json")) {
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    Get-ChildItem $publishDir -File | Where-Object { $_.Name -ne "appsettings.local.json" } |
        Copy-Item -Destination $InstallDir -Force
}
$exe = Join-Path $InstallDir "xck-publisher.exe"
$logsDir = Join-Path $InstallDir "logs"
$sshDir = Join-Path $InstallDir "ssh"
$keyPath = Join-Path $sshDir "id_ed25519"
$knownHosts = Join-Path $sshDir "known_hosts"

# 4. Service : droits de LocalService, git « safe.directory », clé SSH de déploiement
if ($serviceMode) {
    Step "Compte LocalService : droits et accès git"
    $sshExe = "$env:WINDIR\System32\OpenSSH\ssh.exe"
    $keygen = "$env:WINDIR\System32\OpenSSH\ssh-keygen.exe"
    if (-not (Test-Path $sshExe)) {
        # Repli : le ssh de Git for Windows.
        $gitRoot = Split-Path -Parent (Split-Path -Parent $gitCmd.Source)
        $sshExe = Join-Path $gitRoot "usr\bin\ssh.exe"
        $keygen = Join-Path $gitRoot "usr\bin\ssh-keygen.exe"
    }
    if (-not (Test-Path $sshExe)) { Fail "client SSH introuvable (ni OpenSSH de Windows, ni celui de Git for Windows)." }

    if ($PSCmdlet.ShouldProcess($RepoPath, "Donner à LocalService le droit de modifier le dépôt (icacls)")) {
        Invoke-Native icacls @($RepoPath, "/grant", "$($SidLocalService):(OI)(CI)M", "/Q") "icacls (dépôt)"
    }
    if ($PSCmdlet.ShouldProcess($InstallDir, "Donner à LocalService la lecture de l'installation et l'écriture des logs (icacls)")) {
        New-Item -ItemType Directory -Force -Path $logsDir | Out-Null
        Invoke-Native icacls @($InstallDir, "/grant", "$($SidLocalService):(OI)(CI)RX", "/Q") "icacls (installation)"
        Invoke-Native icacls @($logsDir, "/grant", "$($SidLocalService):(OI)(CI)M", "/Q") "icacls (logs)"
    }
    $safeDir = $RepoPath -replace '\\', '/'
    $known = @(& git config --system --get-all safe.directory 2>$null)
    if ($known -notcontains $safeDir -and $known -notcontains '*' -and $PSCmdlet.ShouldProcess($safeDir, "git config --system --add safe.directory")) {
        Invoke-Native git @("config", "--system", "--add", "safe.directory", $safeDir) "git config safe.directory"
    }

    # Clé de déploiement : lisible seulement par LocalService, SYSTEM et Administrateurs ; la clé privée n'est jamais affichée.
    if (-not (Test-Path $keyPath) -and $PSCmdlet.ShouldProcess($keyPath, "Créer la clé SSH de déploiement (ed25519, sans phrase de passe)")) {
        New-Item -ItemType Directory -Force -Path $sshDir | Out-Null
        # '""' : sous PowerShell 5.1, une chaîne vide n'est pas transmise à un programme externe.
        Invoke-Native $keygen @("-t", "ed25519", "-N", '""', "-C", "x-bots service $env:COMPUTERNAME", "-f", $keyPath, "-q") "ssh-keygen"
    }
    if ((Test-Path $sshDir) -and $PSCmdlet.ShouldProcess($sshDir, "Restreindre les droits du dossier de la clé")) {
        foreach ($item in @($sshDir) + @(Get-ChildItem $sshDir -File | ForEach-Object FullName)) {
            Invoke-Native icacls @($item, "/inheritance:r", "/grant:r", "$($SidAdmins):F", "$($SidSystem):F", "$($SidLocalService):RX", "/Q") "icacls (clé)"
            # Propriétaire : Administrateurs (accepté par OpenSSH pour une clé privée).
            Invoke-Native icacls @($item, "/setowner", $SidAdmins, "/Q") "icacls (propriétaire de la clé)"
        }
    }
    # Clés publiques de GitHub, publiées par GitHub (https://api.github.com/meta) : pas de « trust on first use ».
    if ($PSCmdlet.ShouldProcess($knownHosts, "Écrire les clés d'hôte de github.com")) {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $meta = Invoke-RestMethod -Uri "https://api.github.com/meta" -UseBasicParsing
        $lines = $meta.ssh_keys | ForEach-Object { "github.com $_" }
        if (-not $lines) { Fail "impossible de lire les clés d'hôte de GitHub (api.github.com/meta)." }
        [IO.File]::WriteAllLines($knownHosts, [string[]]$lines, (New-Object Text.UTF8Encoding $false))
        Invoke-Native icacls @($knownHosts, "/inheritance:r", "/grant:r", "$($SidAdmins):F", "$($SidSystem):F", "$($SidLocalService):RX", "/Q") "icacls (known_hosts)"
    }
    $sshCommand = "`"$($sshExe -replace '\\','/')`" -i `"$($keyPath -replace '\\','/')`" -o IdentitiesOnly=yes -o StrictHostKeyChecking=yes -o UserKnownHostsFile=`"$($knownHosts -replace '\\','/')`""
}

# 5. appsettings.local.json : créé ou complété (les réglages déjà là sont gardés)
$localSettings = Join-Path $InstallDir "appsettings.local.json"
$cfg = if (Test-Path $localSettings) { Get-Content $localSettings -Raw -Encoding UTF8 | ConvertFrom-Json } else { [pscustomobject]@{} }
if (-not $cfg) { $cfg = [pscustomobject]@{} }
Set-JsonValue $cfg "RepoPath" $RepoPath
Set-JsonValue $cfg "Service.IntervalMinutes" $IntervalMinutes
if ($serviceMode) {
    Set-JsonValue $cfg "Git.Remote" $sshUrl
    Set-JsonValue $cfg "Git.Branch" $branch
    Set-JsonValue $cfg "Git.SshCommand" $sshCommand
    $email = $null
    if ($cfg.PSObject.Properties.Name -contains "Git" -and $cfg.Git.PSObject.Properties.Name -contains "UserEmail") { $email = $cfg.Git.UserEmail }
    if (-not $email) {
        # LocalService n'a pas de configuration git : l'auteur des commits du bot doit être donné.
        $email = (& git -C $RepoPath config user.email 2>$null)
        if (-not $email) { $email = "$ghOwner@users.noreply.github.com" }
        Set-JsonValue $cfg "Git.UserEmail" "$email".Trim()
    }
}
if ($PSCmdlet.ShouldProcess($localSettings, "Écrire RepoPath$(if ($serviceMode) { ', Git.Remote, Git.Branch, Git.SshCommand, Git.UserEmail' }) et Service.IntervalMinutes")) {
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    [IO.File]::WriteAllText($localSettings, ($cfg | ConvertTo-Json -Depth 10) + "`r`n", (New-Object Text.UTF8Encoding $false))
    Write-Host "appsettings.local.json à jour."
}

# 6. Service : clé de déploiement sur GitHub, testée sous le compte LocalService
$script:testDiag = ""
function Test-GitAsLocalService {
    # Tâche planifiée jetable sous LocalService (sans mot de passe), qui lance un « git push --dry-run » et note le résultat.
    $out = Join-Path $logsDir "git-test.txt"
    $cmdFile = Join-Path $logsDir "git-test.cmd"
    Remove-Item $out -ErrorAction SilentlyContinue
    $body = "@echo off`r`nset GIT_TERMINAL_PROMPT=0`r`nset GIT_SSH_COMMAND=$sshCommand`r`n`"$($gitCmd.Source)`" -C `"$RepoPath`" push --dry-run $sshUrl HEAD:refs/heads/$branch > `"$out`" 2>&1`r`necho CODE=%ERRORLEVEL%>> `"$out`"`r`n"
    [IO.File]::WriteAllText($cmdFile, $body, (New-Object Text.ASCIIEncoding))
    $testTask = "X-Bots test git"
    $principal = New-ScheduledTaskPrincipal -UserId "NT AUTHORITY\LOCALSERVICE" -LogonType ServiceAccount
    $action = New-ScheduledTaskAction -Execute "cmd.exe" -Argument "/c `"$cmdFile`""
    Register-ScheduledTask -TaskName $testTask -Action $action -Principal $principal -Force | Out-Null
    try {
        Start-ScheduledTask -TaskName $testTask
        # Le fichier n'existe qu'une fois la commande lancée : Test-Path d'abord (Select-String s'arrête sur un fichier absent).
        for ($i = 0; $i -lt 60; $i++) {
            if ((Test-Path $out) -and (Select-String -Path $out -Pattern '^CODE=' -Quiet)) { break }
            Start-Sleep -Seconds 1
        }
        if (-not (Test-Path $out)) {
            $info = Get-ScheduledTaskInfo -TaskName $testTask
            $script:testDiag = "la commande de test n'a rien écrit en 60 s (tâche de test : résultat 0x{0:X}, lancée le {1})." -f $info.LastTaskResult, $info.LastRunTime
        }
    }
    finally {
        Unregister-ScheduledTask -TaskName $testTask -Confirm:$false
        Remove-Item $cmdFile -ErrorAction SilentlyContinue
    }
    $text = if (Test-Path $out) { Get-Content $out -Raw } else { "$script:testDiag" }
    Remove-Item $out -ErrorAction SilentlyContinue
    # Refusé pour retard sur GitHub (« rejected ») : l'accès en écriture marche quand même.
    $ok = ($text -match 'CODE=0') -or ($text -match 'rejected|up-to-date|->')
    $ok = $ok -and ($text -notmatch 'Permission denied|read only|Host key verification failed|Could not read|fatal:')
    return [pscustomobject]@{ Ok = $ok; Text = $text }
}

if ($serviceMode -and -not $WhatIfPreference) {
    Step "Accès GitHub du service (clé de déploiement)"
    $test = Test-GitAsLocalService
    while (-not $test.Ok) {
        Write-Host "Le service n'a pas encore le droit de pousser sur GitHub. Ajouter cette clé PUBLIQUE au dépôt :" -ForegroundColor Yellow
        Write-Host ""
        Write-Host (Get-Content "$keyPath.pub" -Raw).Trim()
        Write-Host ""
        Write-Host "  1. Ouvrir https://github.com/$ghOwner/$ghRepo/settings/keys/new"
        Write-Host "  2. Title : x-bots service $env:COMPUTERNAME ; Key : coller la ligne ci-dessus"
        Write-Host "  3. Cocher « Allow write access », puis « Add key »"
        Write-Host ""
        Write-Host "Réponse de git sous LocalService :" -ForegroundColor DarkGray
        Write-Host $test.Text -ForegroundColor DarkGray
        $answer = Read-Host "Entrée quand c'est fait (ou « q » pour continuer sans : le service ne pourra pas pousser)"
        if ($answer -match '^q') { Write-Warning "accès GitHub non vérifié : le service publiera, mais ne pourra pas pousser status.json (erreurs dans le log)."; break }
        $test = Test-GitAsLocalService
    }
    if ($test.Ok) { Write-Host "Accès GitHub du service : OK (git push --dry-run sous LocalService)." -ForegroundColor Green }
}

# 7. Test à blanc
if ((Test-Path $exe) -and -not $WhatIfPreference) {
    Step "Test à blanc (--dry-run : rien n'est publié ni modifié)"
    & $exe --dry-run
    if ($LASTEXITCODE -ne 0) { Write-Warning "le dry-run s'est terminé avec le code $LASTEXITCODE : lire les messages ci-dessus avant d'activer le bot." }
}

# 8. Campagnes actives : leur prochain post partira au premier vrai passage
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
    Write-Host "Ces campagnes publieront leur prochain post au premier passage après l'heure indiquée (aujourd'hui si elle est passée, tous à la suite) :"
    $active | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
    if (-not $Force -and -not $WhatIfPreference) {
        $answer = Read-Host "Démarrer le bot maintenant ? (o/N)"
        if ($answer -notmatch '^(o|oui|y|yes)$') { $enable = $false }
    }
}

# 9. Service Windows ou tâche planifiée (jamais les deux)
$startedAt = Get-Date
if ($serviceMode) {
    Step "Service Windows « $ServiceName »"
    if ($task -and $PSCmdlet.ShouldProcess($TaskName, "Supprimer l'ancienne tâche planifiée (le service la remplace)")) {
        Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
        Write-Host "Ancienne tâche planifiée « $TaskName » supprimée."
    }
    if ($PSCmdlet.ShouldProcess($ServiceName, "Créer ou mettre à jour le service (LocalService, démarrage $(if ($enable) { 'automatique différé' } else { 'manuel' }))")) {
        if (-not [Diagnostics.EventLog]::SourceExists($ServiceName)) { New-EventLog -LogName Application -Source $ServiceName }
        $binPath = "`"$exe`" --service"
        if (-not $svc) {
            New-Service -Name $ServiceName -BinaryPathName $binPath -DisplayName "X-Bots Publisher" -StartupType Manual `
                -Description "Publie sur X les posts dus des comptes de $accountsDir, toutes les $IntervalMinutes min (voir src\README.md)." | Out-Null
        }
        else {
            Invoke-Native sc.exe @("config", $ServiceName, "binPath=", $binPath) "sc config binPath"
        }
        # Compte LocalService, sans mot de passe.
        $r = Invoke-CimMethod -ClassName Win32_Service -Filter "Name='$ServiceName'" -MethodName Change -Arguments @{ StartName = "NT AUTHORITY\LocalService"; StartPassword = "" }
        if ($r.ReturnValue -ne 0) { Fail "impossible de passer le service sous LocalService (Win32_Service.Change : $($r.ReturnValue))." }
        # Redémarrage automatique en cas de plantage : après 1 min, 1 min, puis 5 min ; compteur remis à zéro chaque jour.
        Invoke-Native sc.exe @("failure", $ServiceName, "reset=", "86400", "actions=", "restart/60000/restart/60000/restart/300000") "sc failure"
        Invoke-Native sc.exe @("config", $ServiceName, "start=", $(if ($enable) { "delayed-auto" } else { "demand" })) "sc config start"
        if ($enable) {
            Start-Service -Name $ServiceName
            # Le premier passage a lieu tout de suite : last-run.txt doit être réécrit.
            $lastRun = Join-Path $logsDir "last-run.txt"
            for ($i = 0; $i -lt 60; $i++) {
                if ((Test-Path $lastRun) -and (Get-Item $lastRun).LastWriteTime -ge $startedAt) { break }
                Start-Sleep -Seconds 1
            }
            if ((Test-Path $lastRun) -and (Get-Item $lastRun).LastWriteTime -ge $startedAt) {
                Write-Host "Service démarré, premier passage fait :" -ForegroundColor Green
                Write-Host "  $((Get-Content $lastRun -Raw -Encoding UTF8).Trim())"
                Write-Host "Ensuite un passage toutes les $IntervalMinutes minutes (:00, :15, :30, :45), y compris après un redémarrage du PC."
            }
            else {
                Write-Warning "le service ne semble pas avoir fait de passage (last-run.txt pas mis à jour en 60 s)."
                Write-Host "  État du service : $((Get-Service $ServiceName).Status)"
                if (Test-Path (Join-Path $logsDir "service.log")) { Get-Content (Join-Path $logsDir "service.log") -Tail 5 -Encoding UTF8 | ForEach-Object { Write-Host "  service.log : $_" } }
                Get-EventLog -LogName Application -Source $ServiceName, "Service Control Manager" -Newest 5 -After $startedAt -ErrorAction SilentlyContinue |
                    ForEach-Object { Write-Host "  journal Windows : $($_.Message)" }
                Write-Host "  Envoyer ces lignes, et la fin de $logsDir\publisher-*.log s'il existe."
            }
        }
        else {
            Write-Host "Service installé, arrêté (démarrage manuel). Pour le démarrer, et au démarrage du PC ensuite :" -ForegroundColor Yellow
            Write-Host "  sc.exe config $ServiceName start= delayed-auto ; Start-Service $ServiceName"
        }
    }
}
else {
    Step "Tâche planifiée « $TaskName »"
    if ($svc -and $PSCmdlet.ShouldProcess($ServiceName, "Supprimer le service (la tâche le remplace)")) {
        Invoke-Native sc.exe @("delete", $ServiceName) "sc delete"
        Write-Host "Service « $ServiceName » supprimé."
    }
    $action = New-ScheduledTaskAction -Execute $exe -WorkingDirectory $InstallDir
    # Répétition sans fin (pas de RepetitionDuration), à partir de la minute suivante.
    $trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes $IntervalMinutes)
    $settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -StartWhenAvailable `
        -ExecutionTimeLimit (New-TimeSpan -Minutes 10) -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
    if ($PSCmdlet.ShouldProcess($TaskName, "Créer ou mettre à jour la tâche (compte $User)")) {
        $description = "Publie sur X les posts dus des comptes de $accountsDir (voir src\README.md)."
        if ($RunOnlyWhenLoggedOn) {
            $principal = New-ScheduledTaskPrincipal -UserId $User -LogonType Interactive -RunLevel Limited
            Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings `
                -Principal $principal -Description $description -Force | Out-Null
        }
        else {
            # Mot de passe Windows (celui du compte Microsoft si la session en utilise un, pas le code PIN),
            # gardé en mémoire le temps de l'enregistrement, jamais écrit dans un fichier.
            $cred = Get-Credential -UserName $User -Message "Mot de passe Windows de $User (celui du compte Microsoft, pas le code PIN), pour lancer la tâche même sans session ouverte"
            if (-not $cred) { Fail "pas de mot de passe : tâche non créée." }
            Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings `
                -User $cred.UserName -Password $cred.GetNetworkCredential().Password -RunLevel Limited `
                -Description $description -Force | Out-Null
        }
        if ($enable) {
            Enable-ScheduledTask -TaskName $TaskName | Out-Null
            Start-ScheduledTask -TaskName $TaskName
            Start-Sleep -Seconds 10
            $info = Get-ScheduledTaskInfo -TaskName $TaskName
            if ($info.LastTaskResult -eq 0x41303 -or $info.LastRunTime.Year -lt 2000) {
                Write-Warning "Windows n'a pas lancé la tâche. En général : mot de passe refusé (session avec compte Microsoft ou code PIN)."
                Write-Warning "Plus simple : le service, sans mot de passe : .\src\install.cmd -SkipBuild"
            }
            else {
                Write-Host ("Tâche active, premier passage lancé (résultat 0x{0:X} ; 0 = OK, 0x41301 = en cours). Ensuite toutes les $IntervalMinutes minutes." -f $info.LastTaskResult) -ForegroundColor Green
            }
        }
        else {
            Disable-ScheduledTask -TaskName $TaskName | Out-Null
            Write-Host "Tâche créée, désactivée. Pour l'activer : Enable-ScheduledTask -TaskName '$TaskName'" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
if ($waiting.Count -gt 0 -and -not $WhatIfPreference) {
    Write-Host "En attente des clés X pour : $($waiting -join ', '). Rien ne sera publié pour ces comptes tant que leur account.local.json n'est pas rempli." -ForegroundColor Yellow
}
Write-Host "Ce qui a été fait : $logsDir\publisher-AAAA-MM-JJ.log ; dernier passage : $logsDir\last-run.txt"
if ($serviceMode) { Write-Host "Service : Get-Service $ServiceName ; Stop-Service $ServiceName ; Start-Service $ServiceName" }
