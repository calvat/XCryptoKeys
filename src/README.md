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
| `Git.Enabled` | `true` | Pull, commit et push automatiques |
| `Git.Remote` / `Git.Branch` | `origin` / branche courante | Où pousser |
| `Git.UserName` / `Git.UserEmail` | | Auteur des commits du serveur |
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

**Windows : script d'installation.** Le programme fait un passage puis s'arrête : sous Windows, le « service » est une tâche planifiée qui le lance toutes les 15 minutes. `install.ps1` fait tout, dans un PowerShell **administrateur**, depuis le clone du dépôt :

```powershell
.\src\install.cmd                  # installation ou mise à jour
.\src\install.cmd -WhatIf          # montre ce qui serait fait, sans rien changer (sans droits administrateur)
.\src\uninstall.cmd                # supprime la tâche (garde les fichiers)
.\src\uninstall.cmd -RemoveFiles   # supprime aussi C:\xck-publisher
```

`install.cmd` et `uninstall.cmd` lancent `install.ps1` et `uninstall.ps1` avec `-ExecutionPolicy Bypass` : ils marchent même quand Windows bloque les scripts PowerShell (« l'exécution de scripts est désactivée sur ce système »), sans changer ce réglage. Les options sont transmises telles quelles.

`install.ps1` :

1. vérifie les droits administrateur, git, et que le dépôt contient `accounts/` ;
2. construit l'exécutable (comme `publish.ps1` ; `-SkipBuild` pour réutiliser `src/publish/win-x64/`) ;
3. le copie dans `C:\xck-publisher` (`-InstallDir`), sans jamais écraser `appsettings.local.json` ni `logs/`, et crée `appsettings.local.json` avec `RepoPath` s'il n'existe pas ;
4. signale chaque compte sans `account.local.json` (ses clés X ne sont jamais créées ni affichées par le script) ;
5. lance un `--dry-run` et l'affiche ;
6. liste les campagnes en `active` (leur prochain post partira au premier passage) et demande confirmation (`-Force` pour ne pas demander, `-NoStart` pour créer la tâche désactivée) ;
7. crée ou met à jour la tâche « X-Bots Publisher » (`-TaskName`) : toutes les 15 minutes (`-IntervalMinutes`), sans fin, que la session soit ouverte ou non, un seul passage à la fois, rattrapage après un arrêt, 10 minutes maximum par passage.

La tâche tourne avec le compte qui a les **identifiants git** permettant de pousser (par défaut l'utilisateur courant, `-User` pour un autre). Windows demande son mot de passe pour lancer la tâche sans session ouverte : il n'est jamais écrit dans un fichier. Si la session Windows utilise un compte Microsoft ou un code PIN, c'est le mot de passe du compte Microsoft qu'il faut.

Avec `-RunOnlyWhenLoggedOn`, aucun mot de passe n'est demandé : la tâche ne tourne que pendant que la session de ce compte est ouverte (le PC doit rester connecté, écran verrouillé possible).

Après l'enregistrement, le script lance un premier passage et vérifie que Windows a démarré la tâche. Une tâche jamais lancée garde le résultat `0x41303` : c'est en général un mot de passe refusé ; relancer avec le bon mot de passe ou avec `-RunOnlyWhenLoggedOn`.

Relancer `install.ps1` met à jour l'exécutable et la tâche (par exemple après un `git pull`).

À la main, sans le script, dans une invite de commandes administrateur :

```bat
schtasks /Create /TN "X-Bots Publisher" /TR "C:\xck-publisher\xck-publisher.exe" /SC MINUTE /MO 15 /RU "%USERNAME%" /RP *
```

Un verrou empêche deux passages en même temps.

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
