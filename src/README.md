# xck-publisher : publication automatique sur X

Programme C# (.NET 10) qui publie sur X les posts préparés dans `accounts/<Compte>/threads/`, pour tous les comptes du dépôt. Il tourne sur un serveur, lancé toutes les 15 minutes. Pour ajouter une campagne, il suffit de la pousser dans le dépôt et de la passer en `active` : le serveur la récupère au passage suivant. Pour ajouter un compte, voir [Ajouter un compte](#ajouter-un-compte).

## Ce que fait un passage

1. `git pull --rebase` du dépôt. Si le pull échoue, rien n'est publié (sinon on risquerait de republier un post).
2. Pour chaque compte de `accounts/` (un dossier qui contient `threads/`, sauf ceux qui commencent par `_`), avec ses propres clés X, et pour chaque campagne de son `threads/` (sauf `_template`) dont `status.json` est en `active` :
   - le prochain post est `last_posted_index + 1` ;
   - il est dû le jour `last_posted_at + interval_days` à `post_time` (UTC). Pour le premier post, il est dû le jour même à `post_time` ;
   - s'il est dû : texte de la version `simple` (ou de la version indiquée par le champ `"use"` du post), avec le hashtag de campagne ajouté à la fin, et l'image `post_N.png` s'il y en a une ;
   - au plus **un post par campagne et par passage**, quel que soit le nombre de comptes. Après une coupure de plusieurs jours, seul le post suivant part, puis le rythme normal reprend.
3. Après chaque post : mise à jour de `status.json` (`last_posted_index`, `last_posted_at`) et de `posts.json` (`status: posted`, `posted_at`, `tweet_id`, `url`), puis un commit. Après le dernier post d'une campagne, elle repasse en `inactive`.
4. `git push` à la fin du passage (avec rebase et nouvel essai si le dépôt a bougé entre-temps).

Un post est **bloqué** (erreur dans le log, la campagne attend) s'il n'est pas `checked`, si la version demandée n'existe pas, ou s'il dépasse 280 caractères tels que X les compte, hashtag de campagne compris. Les autres campagnes continuent.

Un compte dont les clés X ne sont pas encore renseignées (`account.local.json` absent, vide ou resté à l'exemple) est **en attente** : rien n'est publié pour lui, une ligne d'information dans le log, pas d'erreur. Dès que les 4 clés sont dans le fichier, le passage suivant publie, sans rien réinstaller. Des clés **en partie** renseignées sont une erreur (compte ignoré, erreur dans le log) ; les autres comptes continuent. Les lignes du log sont préfixées par `[Compte/campagne]`.

## Hashtag de campagne

Chaque campagne a un hashtag unique, défini par le compte ; pour XCryptoKeys, `#XCK` + 3 lettres du sujet + numéro de campagne (par exemple `#XCKbtc1`). Il est défini dans `index.md` et dans le champ `hashtag` de `status.json`. Le programme lit `status.json` et l'ajoute à la fin de chaque post s'il n'y est pas déjà. Cliquer dessus sur X affiche les posts de la campagne (onglet « Latest ») ; `from:xcryptokeys #XCKbtc1` dans la recherche est la requête la plus fiable.

Avec le hashtag du préfixe, cela fait **2 hashtags par post** : la règle est de 2 au maximum, donc pas d'autre hashtag dans le texte des posts.

Deux tests vérifient le vrai dépôt, tous comptes confondus, à lancer après avoir écrit une nouvelle campagne :

- `Real_campaigns_all_fit_in_280_with_their_hashtag` : chaque version `simple` tient dans 280 caractères avec le hashtag de campagne ;
- `Real_posts_not_yet_published_have_at_most_two_hashtags` : les versions `simple`, `tech` et `variant` des posts pas encore publiés, hashtag de campagne compris, ont 2 hashtags au plus (la numérotation `#12:` ne compte pas).

## Compiler

Il faut le SDK .NET 10 sur la machine qui compile (pas sur le serveur).

```sh
cd src
dotnet test                      # tests
./publish.sh linux-x64           # Linux  -> src/publish/linux-x64/xck-publisher
.\publish.ps1 -Rid win-x64       # Windows -> src\publish\win-x64\xck-publisher.exe
```

L'exécutable est autonome (un seul fichier, .NET inclus). Copier le dossier `publish/<rid>/` sur le serveur, **hors du dépôt** (par exemple `/opt/xck-publisher/` ou `C:\xck-publisher\`), pour que les `git pull` ne le touchent pas.

## Configuration

À côté de l'exécutable :

- `appsettings.json` : les réglages par défaut (fourni) ;
- `appsettings.local.json` : les réglages propres au serveur (au minimum `RepoPath`), à créer à partir de `appsettings.local.example.json`. Il est ignoré par git ; ne jamais le committer.

Dans le dossier de chaque compte, `accounts/<Compte>/` :

- `account.json` (commité) : `enabled` et `x.username`, jamais de clés ;
- `account.local.json` (**sur le serveur seulement**, ignoré par git) : les 4 clés de l'API X du compte, à créer à partir de `accounts/XCryptoKeys/account.local.example.json`.

Pour un compte, les réglages sont lus dans cet ordre, le dernier l'emporte : `account.json`, `account.local.json`, puis la section `Accounts.<Compte>` de `appsettings(.local).json` et les variables d'environnement. Un fichier du compte illisible ou invalide fait ignorer ce compte (erreur dans le log) ; les autres comptes sont publiés normalement.

Toute clé peut aussi venir d'une variable d'environnement `XCK__Section__Cle`, par exemple `XCK__Accounts__XCryptoKeys__X__ApiKey` ou `XCK__RepoPath`.

| Clé | Défaut | Rôle |
|---|---|---|
| `RepoPath` | | Chemin du clone du dépôt sur le serveur |
| `AccountsFolder` | `accounts` | Dossier des comptes dans le dépôt (un sous-dossier par compte, ses campagnes dans `threads/`) |
| `DefaultVersion` | `simple` | Version publiée par défaut (`simple`, `tech`, `variant`) ; un post peut la changer avec `"use": "tech"` |
| `AttachImages` | `true` | Joint `post_N.png` (ou .jpg, .webp, .gif) s'il existe |
| `AltText` | `true` | Texte alternatif de l'image, tiré de `card` (big, title, sub). Un appel API de plus par image |
| `MaxWeightedLength` | `280` | Longueur maximale, comptée comme X (URL = 23, « ≈ », « → », emoji = 2) |
| `DelayBetweenPostsSeconds` | `30` | Pause entre deux posts d'un même passage |
| `Log.Directory` | `logs` | Dossier des logs, relatif à l'exécutable s'il n'est pas absolu |
| `Log.RetentionDays` | `30` | Les logs plus vieux sont supprimés |
| `Log.Verbose` | `false` | `true` : chaque passage est écrit dans le log, même sans rien à faire (pour vérifier la tâche planifiée) ; sinon seulement ceux où il s'est passé quelque chose |
| `Git.Enabled` | `true` | Commits automatiques de `status.json` / `posts.json` ; `false` : pas de git du tout |
| `Git.Sync` | `true` | `true` : pull au début du passage, push à la fin. `false` : **git local** (commits locaux, jamais de pull ni de push ; synchronisation à la main avec `src\sync.cmd`). Le service Windows est installé en `false` |
| `Git.Executable` | `git` | Chemin de git ; le service (LocalService) n'a pas le PATH de l'utilisateur : `install.ps1` met le chemin complet |
| `Git.Remote` / `Git.Branch` | `origin` / branche courante | Où pousser |
| `Git.UserName` / `Git.UserEmail` | | Auteur des commits du serveur |
| `Git.SshCommand` | | Commande ssh de git pour le bot (`GIT_SSH_COMMAND`) : la clé de déploiement du service, réglée par `install.ps1` |
| `Service.IntervalMinutes` | `15` | Mode `--service` : un passage toutes les N minutes, aligné sur l'heure |
| `X.ApiBaseUrl` | `https://api.x.com` | Adresse de l'API X, commune à tous les comptes |
| `enabled` (`account.json`) | `true` | `false` : le compte est ignoré, sans erreur |
| `x.username` (`account.json`) | nom du dossier | Nom du compte sur X, sert à construire l'URL des posts (`xcryptokeys` pour XCryptoKeys) |
| `x.apiKey`, `x.apiKeySecret`, `x.accessToken`, `x.accessTokenSecret` (`account.local.json`) | | Clés OAuth 1.0a du compte (voir plus bas). Chaque compte a les siennes ; elles ne sont jamais prises d'un autre compte |
| `Accounts.<Compte>.Enabled`, `Accounts.<Compte>.X.…` | | Facultatif : remplace, depuis `appsettings(.local).json` ou une variable d'environnement, une valeur des fichiers du compte |

`<Compte>` est le nom du dossier dans `accounts/`, par exemple `XCryptoKeys` (majuscules indifférentes). Exemple :

`appsettings.local.json`

```json
{
  "RepoPath": "/srv/x-bots"
}
```

`accounts/XCryptoKeys/account.json` (commité)

```json
{
  "enabled": true,
  "x": { "username": "xcryptokeys" }
}
```

`accounts/XCryptoKeys/account.local.json` (sur le serveur seulement)

```json
{
  "x": { "apiKey": "…", "apiKeySecret": "…", "accessToken": "…", "accessTokenSecret": "…" }
}
```

Le bot ne commite que les `status.json` et `posts.json` des campagnes : `account.local.json`, ignoré par git, n'est jamais ajouté, et le `git pull --rebase --autostash` le laisse en place.

### Passage depuis l'ancienne configuration (un seul compte)

Avant, les clés étaient dans la section `X` et les campagnes dans `threads/`. Le programme refuse maintenant de démarrer (code 2) si des clés sont encore dans `X` : les déplacer dans `accounts/XCryptoKeys/account.local.json` (section `x`, voir l'exemple plus haut), puis ne garder que `RepoPath` dans `appsettings.local.json`. Une section `Accounts.XCryptoKeys.X` dans `appsettings.local.json`, ou `XCK__Accounts__XCryptoKeys__X__ApiKey` (ancien `XCK__X__ApiKey`), marche aussi. Supprimer aussi `ThreadsFolder` s'il était réglé.

## Clés de l'API X

À faire pour chaque compte.

1. Sur [developer.x.com](https://developer.x.com), dans le projet et l'app du compte (par exemple @xcryptokeys) : **User authentication settings** → activer OAuth 1.0a, permissions **Read and write**.
2. Dans **Keys and tokens** : copier l'**API Key** et l'**API Key Secret**, puis **générer** l'**Access Token** et l'**Access Token Secret**, connecté en tant que ce compte. Si les tokens ont été générés avant le passage en Read and write, il faut les régénérer, sinon X refuse les posts (403).
3. Sur le serveur, mettre les 4 valeurs dans `accounts/<Compte>/account.local.json` (à partir de `accounts/XCryptoKeys/account.local.example.json`). Ce fichier est ignoré par git : ne jamais le committer.

### Vérifier les clés : `--check-x`

```powershell
C:\xck-publisher\xck-publisher.exe --check-x                 # tous les comptes
C:\xck-publisher\xck-publisher.exe --check-x --account XCryptoKeys
```

Un seul appel en lecture (`GET /2/users/me`), rien n'est publié. Pour chaque compte : la forme des 4 clés (longueur, espaces autour, ressemblance avec un Bearer Token ou un Client ID OAuth 2.0 ; jamais les clés elles-mêmes), l'écart d'horloge avec X (OAuth 1.0a refuse une horloge trop décalée), le compte X auquel les clés appartiennent (comparé à `x.username`) et les droits du jeton (`read-write` attendu). Code de sortie 0 si tout va bien, 1 sinon.

Sur un **401** (clés refusées), les causes habituelles :

- mauvaise paire de clés : il faut l'**API Key** et l'**API Key Secret** (« Consumer Keys ») avec l'**Access Token** et l'**Access Token Secret** ; pas le Bearer Token, ni le Client ID / Client Secret OAuth 2.0 ;
- clés d'une autre app, ou régénérées depuis : recopier les 4 valeurs actuelles ;
- API Key et API Key Secret inversés, ou Access Token et Access Token Secret inversés ;
- horloge du PC décalée.

Sur un **403** : l'app n'est pas en « Read and write » (la passer en Read and write, puis **régénérer** l'Access Token et son secret), ou le projet n'a pas accès à l'offre de l'API.

Quand X refuse les clés d'un compte pendant un passage (401, ou 403 de droits), le bot arrête ce compte tout de suite (les autres continuent) et le **met en pause** : plus d'appel à l'API toutes les 15 minutes. Il reprend dès que `account.local.json` est modifié, après un `--check-x` réussi, ou au redémarrage du service. `last-run.txt` indique « clés X refusées par X (compte en pause, voir xck-publisher --check-x) ».

Appels utilisés : `POST /2/media/upload` (image), `POST /2/media/metadata` (texte alternatif), `POST /2/tweets`. En pay-per-use, chaque passage écrit dans le log le nombre d'appels envoyés : 3 par post avec image, 2 avec `AltText: false`, 1 sans image.

## Accès git du serveur

Le serveur doit pouvoir **pousser** sur le dépôt GitHub :

- soit une clé SSH dédiée, ajoutée comme *deploy key* **avec accès en écriture** dans les réglages du dépôt, et un clone en `git@github.com:...` ;
- soit un token GitHub à accès limité à ce dépôt (Contents : read and write), enregistré dans le gestionnaire d'identifiants de git.

Les commandes git tournent sans invite (`GIT_TERMINAL_PROMPT=0`) : sans identifiants valides, le passage échoue et le log l'indique.

De ton côté, fais un `git pull` avant de modifier un `status.json` ou un `posts.json`, puisque le serveur y écrit.

## Lancer toutes les 15 minutes

**Linux (cron)**, avec `crontab -e` :

```cron
*/15 * * * * /opt/xck-publisher/xck-publisher > /dev/null 2>&1
```

Le programme écrit lui-même son log dans `logs/`.

**Windows : service.** `xck-publisher --service` tourne en continu et fait un passage toutes les `Service.IntervalMinutes` minutes, aligné sur l'heure (:00, :15, :30, :45 pour 15), plus un passage dès le démarrage. Chaque passage est un processus `xck-publisher` à part, comme lancé à la main : les réglages, les `account.local.json` et le dépôt sont relus à chaque passage, et un passage qui plante n'arrête pas le service. À l'arrêt du service, le passage en cours s'arrête entre deux campagnes, jamais au milieu de l'écriture d'un `status.json`. Lancé dans une console, `--service` fait la même chose jusqu'à Ctrl+C. Démarrages, arrêts et passages anormaux sont notés dans `logs/service.log` (et le démarrage / l'arrêt dans le journal Application de Windows).

`install.ps1` installe le service, dans un PowerShell **administrateur**, depuis le clone du dépôt :

```powershell
.\src\install.cmd                  # installation ou mise à jour (service Windows)
.\src\install.cmd -WhatIf          # montre ce qui serait fait, sans rien changer (sans droits administrateur)
.\src\install.cmd -NoStart         # service installé mais arrêté (démarrage manuel)
.\src\install.cmd -ScheduledTask   # ancien mode : tâche planifiée sous le compte de l'utilisateur
.\src\uninstall.cmd                # arrête et supprime le service (et la tâche), garde les fichiers
.\src\uninstall.cmd -RemoveFiles   # supprime aussi C:\xck-publisher (sauf la clé ssh\, -RemoveDeployKey pour elle)
```

`install.cmd` et `uninstall.cmd` lancent `install.ps1` et `uninstall.ps1` avec `-ExecutionPolicy Bypass` : ils marchent même quand Windows bloque les scripts PowerShell (« l'exécution de scripts est désactivée sur ce système »), sans changer ce réglage. Les options sont transmises telles quelles.

Le service tourne sous **LocalService** (pas de mot de passe, pas de session ouverte), par défaut en **git local** (`Git.Sync` à `false`) : il commit dans le clone, ne contacte jamais GitHub, et l'utilisateur synchronise avec `src\sync.cmd` (pull --rebase puis push, avec ses identifiants, en prenant le verrou du bot ; voir le README à la racine). Ce compte n'a pas les droits sur le clone, ni git dans son PATH : `install.ps1` donne les droits, le chemin complet de git (`Git.Executable`, qui ne doit pas être sous `C:\Users`, sinon il faut Git for Windows pour tous les utilisateurs : `winget install --id Git.Git -e --scope machine`) et ajoute le clone à `safe.directory`.

Avec `-GitHubSync`, le service se synchronise lui-même avec GitHub (`Git.Sync` à `true`) grâce à une clé SSH de déploiement ; `install.ps1` fait alors en plus les étapes 4 à 6 ci-dessous :

1. vérifie les droits administrateur, git, que le clone est sur un disque local (LocalService ne voit pas les lecteurs réseau) et que `origin` est un dépôt GitHub ;
2. construit l'exécutable (comme `publish.ps1` ; `-SkipBuild` pour réutiliser `src/publish/win-x64/`) et le copie dans `C:\xck-publisher` (`-InstallDir`), sans jamais écraser `appsettings.local.json`, `logs/` ni `ssh/` ;
3. donne à LocalService (par son SID, quelle que soit la langue de Windows) le droit de modifier le clone et `logs/`, la lecture de `C:\xck-publisher`, et ajoute le clone à `safe.directory` dans la configuration git du système ;
4. crée une clé SSH ed25519 sans phrase de passe dans `C:\xck-publisher\ssh\` (droits : Administrateurs, SYSTEM et LocalService seulement), et `known_hosts` avec les clés d'hôte publiées par GitHub (`api.github.com/meta`, vérification stricte) ;
5. complète `appsettings.local.json` : `RepoPath`, `Service.IntervalMinutes`, et pour git `Remote` (adresse SSH du dépôt, déduite de `origin`), `Branch`, `SshCommand` (la clé du service) et `UserEmail` (auteur des commits : celui de la configuration git de l'utilisateur, sinon l'adresse noreply de GitHub). Le clone lui-même n'est pas modifié : l'utilisateur garde son `origin` et ses identifiants ;
6. affiche la clé **publique** à ajouter sur GitHub (dépôt > Settings > Deploy keys, « Allow write access ») et attend Entrée, sauf si la dernière vérification du service était bonne. La clé privée n'est jamais affichée ;
7. lance un `--dry-run`, liste les campagnes en `active` et demande confirmation (`-Force` pour ne pas demander, `-NoStart` pour un service arrêté en démarrage manuel) ;
8. supprime une éventuelle tâche planifiée « X-Bots Publisher », crée ou met à jour le service `XBotsPublisher` (démarrage automatique différé, redémarrage après 1 min s'il plante), le démarre, lit la vérification git que le service fait à son démarrage sous son propre compte (`xck-publisher --check-git` : `git ls-remote` puis `git push --dry-run`, rien n'est envoyé ; résultat dans `logs/git-check.txt` et `service.log`) et, si GitHub refuse la clé, réaffiche la clé et redémarre le service à chaque Entrée pour réessayer. Puis il vérifie que `last-run.txt` a été réécrit ; sinon, affiche l'état du service, `service.log` et le journal Windows.

Sans accès git, le bot ne publie rien : le `git pull` du début de passage échoue, et `last-run.txt` l'indique (« 1 erreur(s) : git pull impossible ; aucun post publié pendant ce passage. »). `xck-publisher --check-git` peut aussi être lancé à la main pour vérifier l'accès git avec les réglages du bot.

Relancer `install.cmd` met à jour l'exécutable et le service (par exemple après un `git pull`).

**Ancien mode : tâche planifiée** (`-ScheduledTask`), sous le compte de l'utilisateur, qui doit avoir les identifiants git permettant de pousser (par défaut l'utilisateur courant, `-User` pour un autre). Windows demande son mot de passe pour lancer la tâche sans session ouverte : il n'est jamais écrit dans un fichier ; si la session Windows utilise un compte Microsoft ou un code PIN, c'est le mot de passe du compte Microsoft qu'il faut. Avec `-RunOnlyWhenLoggedOn`, aucun mot de passe : la tâche ne tourne que pendant que la session est ouverte. Une tâche jamais lancée garde le résultat `0x41303` (mot de passe refusé, en général). Installer la tâche supprime le service, et inversement.

À la main, sans le script, dans une invite de commandes administrateur :

```bat
schtasks /Create /TN "X-Bots Publisher" /TR "C:\xck-publisher\xck-publisher.exe" /SC MINUTE /MO 15 /RU "%USERNAME%" /RP *
```

Un verrou, dans le `.git` du dépôt, empêche deux passages en même temps, même lancés par deux comptes Windows différents (service et ligne de commande).

## Tester avant de publier

```sh
xck-publisher --dry-run                          # ce qui partirait maintenant, sans rien envoyer ni modifier
xck-publisher --dry-run --account XCryptoKeys    # un seul compte
xck-publisher --dry-run --campaign bitcoin-001   # une seule campagne (ou --campaign XCryptoKeys/bitcoin-001)
xck-publisher --dry-run --now 2026-11-20T09:00Z  # simule une autre heure
```

Le dry-run ne fait ni pull, ni appel à X, ni écriture dans le dépôt. Il affiche le texte exact, sa longueur, l'image et le texte alternatif.

## Démarrer une campagne

Dans `accounts/<Compte>/threads/<campagne>/status.json`, passer `status` à `active`, régler `post_time` (UTC) et `interval_days`, puis commit et push. Le premier post part au premier passage après `post_time`, le jour même si l'heure est déjà passée. Pour une pause, repasser en `inactive`.

## Ajouter un compte

1. Créer `accounts/<Compte>/threads/` dans le dépôt, avec ses campagnes (partir de `accounts/XCryptoKeys/threads/_template/`), et `accounts/<Compte>/account.json` avec `x.username`, le nom du compte sur X. Rien à changer dans `src/`.
2. Générer les clés X du compte (voir [Clés de l'API X](#clés-de-lapi-x)) et les mettre sur le serveur dans `accounts/<Compte>/account.local.json` (ignoré par git).
3. Vérifier avec `xck-publisher --dry-run --account <Compte>`, puis passer une campagne en `active`.

## Codes de sortie et logs

- `0` : passage normal (y compris quand rien n'était dû) ;
- `1` : au moins une erreur (post bloqué, échec d'API, git) : voir le log ;
- `2` : réglages invalides.

Les logs sont dans `logs/`, à côté de l'exécutable :

- `publisher-AAAA-MM-JJ.log` : **ce qui a été fait**. Seuls les passages où il s'est passé quelque chose y sont écrits : un post publié, une campagne terminée, un avertissement ou une erreur. Le passage est alors écrit en entier (début, lignes, résumé), suivi d'une ligne vide. Un passage sans rien à faire (rien de dû, comptes en attente de leurs clés, campagnes inactives) n'écrit rien. Avec `Log.Verbose` à `true`, chaque passage est écrit ;
- `last-run.txt` : **le dernier passage**, réécrit à chaque fois (une ligne : heure UTC, résultat, prochain post dû). Pour vérifier que la tâche planifiée tourne bien, même quand il n'y a rien à faire.

Exemple de `publisher-AAAA-MM-JJ.log` pour un passage qui publie :

```text
2026-11-12T08:30:04Z INFO  Début du passage, heure de référence 2026-11-12T08:30:04Z, version par défaut « simple ».
2026-11-12T08:30:06Z INFO  API X POST /2/media/upload -> 200
2026-11-12T08:30:06Z INFO  API X POST /2/media/metadata -> 200
2026-11-12T08:30:07Z INFO  API X POST /2/tweets -> 201
2026-11-12T08:30:07Z DONE  [XCryptoKeys/bitcoin-001] post #1/71 publié : https://x.com/xcryptokeys/status/…
2026-11-12T08:30:09Z INFO  Fin du passage : 1 post(s) publié(s), 3 appel(s) à l'API X, 0 erreur(s), 0 avertissement(s).
```

Les lignes d'avant le premier événement (début du passage, campagnes pas encore dues…) sont gardées en mémoire puis écrites avec lui ; ensuite, chaque ligne est écrite tout de suite, et une erreur inattendue est écrite avant l'arrêt du programme. Le `--dry-run` n'écrit que dans la console.

`last-run.txt`, par exemple :

```text
2026-11-12T08:45:02Z rien à faire ; prochain post dû : XCryptoKeys/ethereum-001 #1 le 2026-11-12 10:00 UTC
```

Si le log affiche « POST #N PUBLIÉ … MAIS status.json/posts.json NON MIS À JOUR », le post est en ligne mais l'état n'a pas pu être écrit : mets `last_posted_index` à N dans `status.json` avant le passage suivant, sinon le post repartira.
