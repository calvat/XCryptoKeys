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

## Installer le bot sur un serveur Windows

Dans un PowerShell administrateur, depuis le clone : `.\src\install.ps1`. Le script construit le programme, le copie dans `C:\xck-publisher` et crée la tâche planifiée qui le lance toutes les 15 minutes ; `.\src\uninstall.ps1` la supprime. Détails dans [`src/README.md`](src/README.md#lancer-toutes-les-15-minutes).
