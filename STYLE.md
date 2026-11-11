# Règles de rédaction

## Choisir un sujet de thread

Un sujet n'est retenu que s'il :
- se découpe en **petites thématiques**, chacune tenant dans un post de moins de 280 caractères ;
- tient **au moins 30 posts et au plus 100**, sans remplissage (chaque post ayant au moins 2 versions, cela fait 60 à 200 textes) ;
- est **assez technique pour être intéressant**, mais reste compréhensible par quelqu'un qui n'est pas développeur.

## Écrire un post

- En **anglais**, **moins de 280 caractères**, idéalement 150 à 220.
- **Une seule info technique par post**, compréhensible seule (un post peut être partagé hors du thread).
- **Ordre de rédaction, pour chaque post.** L'idée est d'utiliser le fonctionnement des LLM : un modèle écrit dans l'ordre, et ce qu'il a déjà écrit guide la suite. On le force donc à réfléchir d'abord, puis à reformuler. Les champs de `posts.json` sont dans cet ordre, et un générateur doit les remplir dans cet ordre, jamais `simple` avant `long` :
  1. `long` : on résume d'abord, en 500 caractères maximum, tout ce qu'on cherche à dire (le fait, le chiffre, le contexte, pourquoi c'est intéressant). C'est la référence du post : les autres versions en sont tirées.
  2. `tech` : ensuite seulement, la version technique, tirée de `long`.
  3. `simple` : en dernier, la version simple, tirée de `long` et de `tech`.
- Chaque post est proposé en **au moins 2 versions** (`simple` et `tech` dans `posts.json`), parfois 3 :
  - `simple` : pour tout le monde, sans jargon ou avec le jargon expliqué ;
  - `tech` : plus précis, avec les chiffres et les termes exacts ;
  - `variant` (optionnel) : une variante, par exemple plus accrocheuse ou avec un autre angle.
- Johan choisit une version par post au moment de publier.
- **Affirmations vérifiées** : chaque chiffre, date ou nom a une source notée dans le champ `source` du post (lien, BIP, EIP, rapport post-mortem). Toutes les versions d'un post doivent être vraies, la version simple aussi.
- Un chiffre approximatif s'écrit avec "about" ou "≈".
- Pas de conseil financier, pas de prédiction de prix.
- Le post 1 accroche, le dernier résume et invite à suivre.
- **Publication : 1 post de chaque thread par jour.** Chaque post doit donc se lire seul, sans le post de la veille.

## Préfixe

- Chaque post commence par un **préfixe identique dans tout le thread**, suivi du numéro : par exemple `Inside #Pactus #12:`.
- Le préfixe est **court et percutant**, **propre au sujet** (pas générique comme "Your Keys") et **différent pour chaque thread**.
- Règle simple : **en lisant le préfixe, on doit comprendre que ça touche l'univers crypto** (crypto, web3, blockchain, DeFi...). S'il est trop général ou ambigu, ajouter un mot (par exemple `Crypto #Anonymity Myths #N:`) ou le changer.
- Au moins un mot du préfixe est un **hashtag très utilisé** (comme `#Pactus`, `#Bitcoin`, `#DeFi`, `#Wallet`).
- Il compte dans les 280 caractères : viser 30 caractères au plus.
- La liste des préfixes validés est dans `SUJETS.md`.

## Hashtags fixes

- Chaque série a **1 à 3 hashtags fixes** (2 en général), présents dans **tous** ses posts, en fin de post (comme `#PAC @PactusChain` pour Pactus).
- Ils sont choisis parmi les **hashtags les plus utilisés**, pour que les posts ressortent dans la recherche par hashtag.
- Ils ne répètent pas le hashtag du préfixe.
- Ils **collent au sujet** de la série : pas de #Bitcoin ou #Ethereum dans une série qui ne parle pas surtout de ces chaînes.
- Ils sont notés dans `SUJETS.md`, à côté du préfixe du thème. Ils comptent dans les 280 caractères.
