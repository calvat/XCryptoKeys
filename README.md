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

Le bot est un programme qui fait un passage puis s'arrête. Une tâche planifiée Windows le lance toutes les 15 minutes : à chaque passage, il publie les posts dus des campagnes `active` de chaque compte, puis commit et pousse les `status.json` sur GitHub.

On peut l'installer **avant d'avoir les clés de l'API X** : tant que `account.local.json` n'existe pas ou n'est pas rempli, le bot ne publie rien pour ce compte (le log indique à chaque passage « clés X non renseignées … compte en attente, rien n'est publié », sans erreur). Il suffit ensuite de remplir ce fichier : le passage suivant publie, sans rien réinstaller.

### Ce qu'il faut sur le PC

- **Git for Windows**, avec le droit de **pousser** sur le dépôt (`git push` doit marcher sans rien demander) ;
- le **SDK .NET 10**, pour construire l'exécutable : `winget install Microsoft.DotNet.SDK.10`. Sans lui : copier le dossier `src\publish\win-x64\` construit sur un autre PC au même endroit du clone, et ajouter `-SkipBuild` aux commandes `install.cmd` ;
- un compte Windows avec mot de passe : la tâche tourne sous ce compte, même sans session ouverte.

### Installation

Dans un **PowerShell administrateur** (clic droit sur PowerShell > Exécuter en tant qu'administrateur), dans le clone du dépôt (par exemple `C:\x-bots`) :

1. Mettre le dépôt à jour et vérifier que git peut pousser :

   ```powershell
   cd C:\x-bots
   git pull
   git push --dry-run
   ```

   Si `git push` demande de se connecter, le faire une fois, **avec le compte Windows qui lancera la tâche** : Git for Windows retient les identifiants.

2. Vérifier sans rien changer (pas besoin d'être administrateur pour cette étape) :

   ```powershell
   .\src\install.cmd -WhatIf
   ```

   Passer par `install.cmd` évite l'erreur « l'exécution de scripts est désactivée sur ce système », sans changer le réglage du PC. Équivalent : `powershell -ExecutionPolicy Bypass -File .\src\install.ps1 -WhatIf`.

3. Installer, dans le PowerShell **administrateur** :

   ```powershell
   .\src\install.cmd
   ```

   Le script construit l'exécutable, le copie dans `C:\xck-publisher`, y crée `appsettings.local.json` (chemin du dépôt), lance un test à blanc (`--dry-run`), liste les campagnes actives et demande s'il faut activer la tâche. Il demande ensuite le mot de passe Windows, pour que la tâche tourne même sans session ouverte ; il n'est enregistré dans aucun fichier.

   Sans les clés X, on peut répondre `o` : la tâche tourne mais ne publie rien.

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

**Attention :** au premier passage avec les clés, le bot rattrape tous les posts du jour dont l'heure est passée (un toutes les 30 secondes). Les campagnes sont réparties de 07:00 à 22:00 UTC (08:00 à 23:00 à Paris en hiver). Pour garder cet étalement dès le premier jour, ajouter les clés **avant 07:00 UTC**, ou désactiver la tâche jusque-là (voir plus bas).

### Au quotidien

| Pour… | Commande (PowerShell administrateur) |
|---|---|
| Voir le log | ouvrir `C:\xck-publisher\logs\publisher-AAAA-MM-JJ.log` |
| Tester ce qui partirait maintenant | `C:\xck-publisher\xck-publisher.exe --dry-run` |
| Lancer un passage tout de suite | `Start-ScheduledTask -TaskName 'X-Bots Publisher'` |
| Mettre en pause / reprendre | `Disable-ScheduledTask -TaskName 'X-Bots Publisher'` / `Enable-ScheduledTask -TaskName 'X-Bots Publisher'` |
| Mettre à jour après un `git pull` | relancer `.\src\install.cmd` |
| Désinstaller | `.\src\uninstall.cmd` (ajouter `-RemoveFiles` pour supprimer aussi `C:\xck-publisher`) |

Pour mettre une campagne en pause, passer son `status.json` en `inactive`, puis commit et push. Depuis un autre PC, faire un `git pull` avant de modifier un `status.json` ou un `posts.json`, puisque le bot y écrit.

Détails (options du script, réglages, codes de sortie) : [`src/README.md`](src/README.md#lancer-toutes-les-15-minutes).
