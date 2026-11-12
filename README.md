# X-Bots

Bot de publication automatique sur X, pour plusieurs comptes. Le code du bot est commun ; chaque compte a son dossier dans `accounts/` avec ses campagnes (threads) et ses règles éditoriales.

## Organisation

| Dossier | Contenu |
|---|---|
| `src/` | Le programme C# qui publie automatiquement les posts sur X, pour tous les comptes (voir [`src/README.md`](src/README.md)) |
| `design/` | Les scripts de rendu des images (`card.py`, `render.js`), communs à tous les comptes |
| `accounts/<Compte>/` | Un dossier par compte X : son `README.md`, ses règles (`STYLE.md`, `SUJETS.md`…) et ses campagnes dans `threads/` |
| `accounts/<Compte>/threads/` | Un dossier par campagne, nommé `<nom technique>-<numéro sur 3 chiffres>`, avec `index.md`, `posts.json` et `status.json` ; `_template/` est le modèle à copier |

## Comptes

| Compte | X | Contenu |
|---|---|---|
| [XCryptoKeys](accounts/XCryptoKeys/README.md) | [@xcryptokeys](https://x.com/xcryptokeys) | Blockchains et crypto en général |

## Ajouter un compte

1. Créer `accounts/<Compte>/threads/` (copier `accounts/XCryptoKeys/threads/_template/` pour la première campagne), un `README.md` qui décrit le compte et un `account.json` avec son nom sur X (copier celui de XCryptoKeys).
2. Sur le serveur, mettre ses clés X dans `accounts/<Compte>/account.local.json`, à créer à partir de `accounts/XCryptoKeys/account.local.example.json`. Ce fichier est ignoré par git : les clés ne sont jamais commitées (voir [`src/README.md`](src/README.md#configuration)).
3. Passer une campagne en `active` dans son `status.json`, puis commit et push : le bot la publie au passage suivant.

Le format de `posts.json` et de `status.json` est décrit dans le README de chaque compte (par exemple [celui de XCryptoKeys](accounts/XCryptoKeys/README.md)).

## Installation (Windows)

Le bot tourne comme **service Windows** « X-Bots Publisher » (`XBotsPublisher`), sous le compte **LocalService** : pas de mot de passe, pas besoin de session ouverte, il redémarre avec le PC. Toutes les 15 minutes (à :00, :15, :30, :45), il fait un passage : il publie les posts dus des campagnes `active` de chaque compte, et commit les `status.json` / `posts.json` **dans le clone local**. Il ne contacte jamais GitHub : la synchronisation se fait à la main, avec `.\src\sync.cmd` (voir [Synchroniser avec GitHub](#synchroniser-avec-github)).

On peut l'installer **avant d'avoir les clés de l'API X** : tant que `account.local.json` n'existe pas ou n'est pas rempli, le bot ne publie rien pour ce compte (sans erreur ; rien n'est écrit dans le log, et `C:\xck-publisher\logs\last-run.txt` indique « en attente des clés X : XCryptoKeys »). Il suffit ensuite de remplir ce fichier : le passage suivant publie, sans rien réinstaller.

### Ce qu'il faut sur le PC

- **Git for Windows** ;
- le **SDK .NET 10**, pour construire l'exécutable : `winget install Microsoft.DotNet.SDK.10`. Sans lui : copier le dossier `src\publish\win-x64\` construit sur un autre PC au même endroit du clone, et ajouter `-SkipBuild` aux commandes `install.cmd` ;
- le clone du dépôt sur un **disque local** (pas un lecteur réseau : le service ne le verrait pas).

### Installation

Dans un **PowerShell administrateur** (clic droit sur PowerShell > Exécuter en tant qu'administrateur), dans le clone du dépôt (par exemple `C:\x-bots`) :

1. Mettre le dépôt à jour :

   ```powershell
   cd C:\x-bots
   git pull
   ```

2. Vérifier sans rien changer (pas besoin d'être administrateur pour cette étape) :

   ```powershell
   .\src\install.cmd -WhatIf
   ```

   Passer par `install.cmd` évite l'erreur « l'exécution de scripts est désactivée sur ce système », sans changer le réglage du PC. Équivalent : `powershell -ExecutionPolicy Bypass -File .\src\install.ps1 -WhatIf`.

3. Installer :

   ```powershell
   .\src\install.cmd
   ```

   Le script :

   - construit l'exécutable et le copie dans `C:\xck-publisher` ;
   - donne au compte LocalService le droit de modifier le clone et d'écrire les logs, et déclare le clone « sûr » pour git ;
   - indique au service le chemin complet de git (le service n'a pas le PATH de l'utilisateur) et le règle en **git local** (`Git.Sync` à `false` dans `C:\xck-publisher\appsettings.local.json`) ;
   - lance un test à blanc (`--dry-run`), liste les campagnes actives et demande « Démarrer le bot maintenant ? (o/N) » ;
   - crée le service (démarrage automatique différé, redémarrage s'il plante) et le démarre. À son démarrage, le service vérifie qu'il peut utiliser git dans le clone (`C:\xck-publisher\logs\git-check.txt`) ; le script affiche le résultat et vérifie que le premier passage a eu lieu (`last-run.txt`).

   Avec `N`, le service est installé mais arrêté (démarrage manuel) : `sc.exe config XBotsPublisher start= delayed-auto ; Start-Service XBotsPublisher` pour le démarrer. Sans les clés X, on peut répondre `o` : le service tourne mais ne publie rien.

   Le clone garde son adresse et tes identifiants git : c'est toi qui synchronises avec GitHub. Si une installation précédente avait créé une clé de déploiement (`C:\xck-publisher\ssh\`), le script le signale : elle ne sert plus, on peut la supprimer sur GitHub (Settings > Deploy keys) et sur le PC. La synchronisation par le service lui-même reste possible avec `.\src\install.cmd -GitHubSync` (clé de déploiement, voir [`src/README.md`](src/README.md#lancer-toutes-les-15-minutes)).

   S'il restait une tâche planifiée « X-Bots Publisher » d'une installation précédente, le script la supprime : le bot ne tourne jamais deux fois. L'ancien mode reste possible avec `.\src\install.cmd -ScheduledTask` (voir [`src/README.md`](src/README.md#lancer-toutes-les-15-minutes)).

### Synchroniser avec GitHub

Le service commit ses publications dans le clone, sans les pousser. Pour envoyer ces commits sur GitHub et récupérer tes changements (nouvelles campagnes, `status.json` modifiés ailleurs), sur le PC du bot, dans le clone (PowerShell normal) :

```powershell
.\src\sync.cmd
```

Le script attend la fin d'un passage en cours et bloque le suivant le temps de la synchronisation (même verrou que le bot), puis fait `git pull --rebase` et `git push` avec tes identifiants. À lancer après avoir poussé des changements depuis un autre PC, et de temps en temps pour envoyer les publications du bot (une fois par jour suffit).

Un changement fait ailleurs n'arrive au bot qu'après un `sync.cmd` sur son PC. Si un `status.json` a été modifié des deux côtés, le pull peut s'arrêter sur un conflit : garder la version du bot pour `last_posted_index`, `last_posted_at` et les champs des posts publiés (`status`, `posted_at`, `tweet_id`, `url`), c'est ce qui est vraiment en ligne ; puis `git add <fichier>`, `git rebase --continue` et relancer `sync.cmd` (ou `git rebase --abort` pour tout annuler). Tant que le rebase n'est pas terminé, le bot ne publie rien.

### Ajouter les clés X (plus tard)

1. Sur [developer.x.com](https://developer.x.com), dans l'app du compte (@xcryptokeys) : activer OAuth 1.0a avec les permissions **Read and write**, puis récupérer l'**API Key**, l'**API Key Secret**, et générer l'**Access Token** et l'**Access Token Secret** (après le passage en Read and write ; détails dans [Clés de l'API X](src/README.md#clés-de-lapi-x)).
2. Dans le clone, copier `accounts\XCryptoKeys\account.local.example.json` en `accounts\XCryptoKeys\account.local.json` (même dossier) et y mettre les 4 valeurs :

   ```json
   {
     "x": {
       "apiKey": "…",
       "apiKeySecret": "…",
       "accessToken": "…",
       "accessTokenSecret": "…"
     }
   }
   ```

   Ce fichier est ignoré par git : les clés ne partent jamais sur GitHub.

3. C'est tout : le passage suivant (dans les 15 minutes) publie les posts dus.

**Attention :** au premier passage avec les clés, le bot rattrape tous les posts du jour dont l'heure est passée (un toutes les 30 secondes). Les campagnes sont réparties de 07:00 à 22:00 UTC (08:00 à 23:00 à Paris en hiver). Pour garder cet étalement dès le premier jour, ajouter les clés **avant 07:00 UTC**, ou arrêter le service jusque-là (`Stop-Service XBotsPublisher`, voir plus bas).

### Au quotidien

| Pour… | Commande (PowerShell administrateur) |
|---|---|
| Voir ce qui a été fait | ouvrir `C:\xck-publisher\logs\publisher-AAAA-MM-JJ.log` : seuls les passages où il s'est passé quelque chose (post publié, erreur…) y sont écrits |
| Vérifier que le bot tourne | `Get-Service XBotsPublisher` (doit être `Running`), et `C:\xck-publisher\logs\last-run.txt` : heure et résultat du dernier passage, prochain post dû. Démarrages et arrêts du service : `C:\xck-publisher\logs\service.log` |
| Tester ce qui partirait maintenant | `C:\xck-publisher\xck-publisher.exe --dry-run` |
| Lancer un passage tout de suite | `Restart-Service XBotsPublisher` (le service fait un passage dès qu'il démarre) |
| Mettre en pause / reprendre | `Stop-Service XBotsPublisher` / `Start-Service XBotsPublisher` |
| Synchroniser avec GitHub | `.\src\sync.cmd` dans le clone (PowerShell normal) : envoie les publications du bot, récupère tes changements |
| Mettre à jour après un `git pull` | relancer `.\src\install.cmd` : il arrête le service, remplace l'exécutable et le redémarre |
| Désinstaller | `.\src\uninstall.cmd` (ajouter `-RemoveFiles` pour supprimer aussi `C:\xck-publisher`, et `-RemoveDeployKey` pour la clé de déploiement ; la supprimer aussi sur GitHub > Settings > Deploy keys) |

Pour mettre une campagne en pause, passer son `status.json` en `inactive`, puis commit et push. Depuis un autre PC, faire un `git pull` avant de modifier un `status.json` ou un `posts.json`, puisque le bot y écrit.

Détails (options du script, réglages, codes de sortie) : [`src/README.md`](src/README.md#lancer-toutes-les-15-minutes).
