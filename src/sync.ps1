# Synchronise le clone avec GitHub (git pull --rebase, puis git push), avec tes identifiants git, sans croiser un
# passage du bot : le script prend le même verrou que le bot (.git\xck-publisher.lock). Pendant la synchronisation,
# le bot passe son tour (il réessaie au passage suivant).
# À lancer depuis le clone, dans un PowerShell normal (pas besoin d'être administrateur) :
#   .\src\sync.cmd
param(
    # Vide = le dépôt qui contient ce script.
    [string]$RepoPath = "",
    [string]$Remote = "origin",
    # Attente maximale d'un passage en cours.
    [int]$WaitMinutes = 10
)
$ErrorActionPreference = "Stop"
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $RepoPath) { $RepoPath = Split-Path -Parent $scriptDir }
$RepoPath = (Resolve-Path $RepoPath).Path
$gitDir = Join-Path $RepoPath ".git"
if (-not (Test-Path $gitDir)) { Write-Host "ERREUR : $RepoPath n'est pas un clone git." -ForegroundColor Red; exit 1 }

# Prend le verrou du bot (attend la fin d'un passage en cours).
$lockPath = Join-Path $gitDir "xck-publisher.lock"
$lock = $null
$deadline = (Get-Date).AddMinutes($WaitMinutes)
$told = $false
while (-not $lock) {
    try {
        $lock = [IO.File]::Open($lockPath, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    }
    catch [System.IO.IOException] {
        if (-not $told) { Write-Host "Un passage du bot est en cours : j'attends qu'il se termine…"; $told = $true }
        if ((Get-Date) -gt $deadline) { Write-Host "ERREUR : le passage du bot dure depuis plus de $WaitMinutes min ; réessayer plus tard." -ForegroundColor Red; exit 1 }
        Start-Sleep -Seconds 2
    }
}

try {
    $branch = (& git -C $RepoPath rev-parse --abbrev-ref HEAD).Trim()
    Write-Host "== git pull --rebase $Remote $branch" -ForegroundColor Cyan
    & git -C $RepoPath pull --rebase --autostash $Remote $branch
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Le pull ne s'est pas terminé (conflit ?). Tant que ce n'est pas réglé, le bot ne publie rien." -ForegroundColor Yellow
        Write-Host "  - voir les fichiers en conflit : git status"
        Write-Host "  - pour un status.json ou un posts.json : garder la version du bot pour last_posted_index, last_posted_at"
        Write-Host "    et les champs status/posted_at/tweet_id/url des posts publiés (c'est ce qui est vraiment en ligne) ;"
        Write-Host "    puis git add <fichier> et git rebase --continue"
        Write-Host "  - ou tout annuler : git rebase --abort (le dépôt revient comme avant le pull)"
        Write-Host "  Ensuite relancer .\src\sync.cmd"
        exit 1
    }
    Write-Host "== git push $Remote $branch" -ForegroundColor Cyan
    & git -C $RepoPath push $Remote $branch
    if ($LASTEXITCODE -ne 0) { Write-Host "ERREUR : git push a échoué (voir ci-dessus) ; les commits du bot restent en local, rien n'est perdu." -ForegroundColor Red; exit 1 }
    Write-Host ""
    Write-Host "Synchronisé avec GitHub." -ForegroundColor Green
    & git -C $RepoPath log --oneline -5
}
finally {
    $lock.Dispose()
}
