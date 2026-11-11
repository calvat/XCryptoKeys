# xck-publisher : publication automatique sur X

Programme C# (.NET 10) qui publie sur X les posts préparés dans `threads/`. Il tourne sur un serveur, lancé toutes les 15 minutes. Pour ajouter une campagne, il suffit de la pousser dans le dépôt et de la passer en `active` : le serveur la récupère au passage suivant.

## Ce que fait un passage

1. `git pull --rebase` du dépôt. Si le pull échoue, rien n'est publié (sinon on risquerait de republier un post).
2. Pour chaque campagne de `threads/` (sauf `_template`) dont `status.json` est en `active` :
   - le prochain post est `last_posted_index + 1` ;
   - il est dû le jour `last_posted_at + interval_days` à `post_time` (UTC). Pour le premier post, il est dû le jour même à `post_time` ;
   - s'il est dû : texte de la version `simple` (ou de la version indiquée par le champ `"use"` du post), avec le hashtag de campagne ajouté à la fin, et l'image `post_N.png` s'il y en a une ;
   - au plus **un post par campagne et par passage**. Après une coupure de plusieurs jours, seul le post suivant part, puis le rythme normal reprend.
3. Après chaque post : mise à jour de `status.json` (`last_posted_index`, `last_posted_at`) et de `posts.json` (`status: posted`, `posted_at`, `tweet_id`, `url`), puis un commit. Après le dernier post d'une campagne, elle repasse en `inactive`.
4. `git push` à la fin du passage (avec rebase et nouvel essai si le dépôt a bougé entre-temps).

Un post est **bloqué** (erreur dans le log, la campagne attend) s'il n'est pas `checked`, si la version demandée n'existe pas, ou s'il dépasse 280 caractères tels que X les compte, hashtag de campagne compris. Les autres campagnes continuent.

## Hashtag de campagne

Chaque campagne a un hashtag unique, `#XCK` + 3 lettres du sujet + numéro de campagne (par exemple `#XCKbtc1`). Il est défini dans `index.md` et dans le champ `hashtag` de `status.json`. Le programme lit `status.json` et l'ajoute à la fin de chaque post s'il n'y est pas déjà. Cliquer dessus sur X affiche les posts de la campagne (onglet « Latest ») ; `from:xcryptokeys #XCKbtc1` dans la recherche est la requête la plus fiable.

Le test `Real_campaigns_all_fit_in_280_with_their_hashtag` vérifie que toutes les versions `simple` du dépôt tiennent dans 280 caractères avec leur hashtag. À lancer après avoir écrit une nouvelle campagne.

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
- `appsettings.local.json` : les réglages du serveur et les **secrets**, à créer à partir de `appsettings.local.example.json`. Il est ignoré par git ; ne jamais le committer.

Toute clé peut aussi venir d'une variable d'environnement `XCK__Section__Cle`, par exemple `XCK__X__ApiKey` ou `XCK__RepoPath`.

| Clé | Défaut | Rôle |
|---|---|---|
| `RepoPath` | | Chemin du clone du dépôt sur le serveur |
| `ThreadsFolder` | `threads` | Dossier des campagnes dans le dépôt |
| `DefaultVersion` | `simple` | Version publiée par défaut (`simple`, `tech`, `variant`) ; un post peut la changer avec `"use": "tech"` |
| `AttachImages` | `true` | Joint `post_N.png` (ou .jpg, .webp, .gif) s'il existe |
| `AltText` | `true` | Texte alternatif de l'image, tiré de `card` (big, title, sub). Un appel API de plus par image |
| `MaxWeightedLength` | `280` | Longueur maximale, comptée comme X (URL = 23, « ≈ », « → », emoji = 2) |
| `DelayBetweenPostsSeconds` | `30` | Pause entre deux posts d'un même passage |
| `Log.Directory` | `logs` | Dossier des logs, relatif à l'exécutable s'il n'est pas absolu |
| `Log.RetentionDays` | `30` | Les logs plus vieux sont supprimés |
| `Git.Enabled` | `true` | Pull, commit et push automatiques |
| `Git.Remote` / `Git.Branch` | `origin` / branche courante | Où pousser |
| `Git.UserName` / `Git.UserEmail` | | Auteur des commits du serveur |
| `X.Username` | `xcryptokeys` | Sert à construire l'URL des posts |
| `X.ApiKey`, `X.ApiKeySecret`, `X.AccessToken`, `X.AccessTokenSecret` | | Clés OAuth 1.0a (voir plus bas) |

## Clés de l'API X

1. Sur [developer.x.com](https://developer.x.com), dans le projet et l'app du compte @xcryptokeys : **User authentication settings** → activer OAuth 1.0a, permissions **Read and write**.
2. Dans **Keys and tokens** : copier l'**API Key** et l'**API Key Secret**, puis **générer** l'**Access Token** et l'**Access Token Secret**, connecté en tant que @xcryptokeys. Si les tokens ont été générés avant le passage en Read and write, il faut les régénérer, sinon X refuse les posts (403).
3. Mettre les 4 valeurs dans `appsettings.local.json`.

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

**Windows (Planificateur de tâches)**, dans une invite de commandes administrateur :

```bat
schtasks /Create /TN "XCryptoKeys Publisher" /TR "C:\xck-publisher\xck-publisher.exe" /SC MINUTE /MO 15 /RU "%USERNAME%"
```

La tâche doit tourner avec un compte qui a les identifiants git. Un verrou empêche deux passages en même temps.

## Tester avant de publier

```sh
xck-publisher --dry-run                          # ce qui partirait maintenant, sans rien envoyer ni modifier
xck-publisher --dry-run --campaign bitcoin-001   # une seule campagne
xck-publisher --dry-run --now 2026-11-20T09:00Z  # simule une autre heure
```

Le dry-run ne fait ni pull, ni appel à X, ni écriture dans le dépôt. Il affiche le texte exact, sa longueur, l'image et le texte alternatif.

## Démarrer une campagne

Dans `threads/<campagne>/status.json`, passer `status` à `active`, régler `post_time` (UTC) et `interval_days`, puis commit et push. Le premier post part au premier passage après `post_time`, le jour même si l'heure est déjà passée. Pour une pause, repasser en `inactive`.

## Codes de sortie et logs

- `0` : passage normal (y compris quand rien n'était dû) ;
- `1` : au moins une erreur (post bloqué, échec d'API, git) : voir le log ;
- `2` : réglages invalides.

Les logs sont dans `logs/publisher-AAAA-MM-JJ.log`, à côté de l'exécutable.

Si le log affiche « POST #N PUBLIÉ … MAIS status.json/posts.json NON MIS À JOUR », le post est en ligne mais l'état n'a pas pu être écrit : mets `last_posted_index` à N dans `status.json` avant le passage suivant, sinon le post repartira.
