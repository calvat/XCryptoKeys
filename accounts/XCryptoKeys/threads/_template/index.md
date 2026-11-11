# Campagne <nom>-<NNN> : <titre>

- Sujet : <numéro et nom dans SUJETS.md>
- Angle : <une phrase>
- Préfixe : `<préfixe du thème> #N:` (voir SUJETS.md)
- Hashtags fixes : <1 à 3, voir SUJETS.md>
- Hashtag de campagne : `#XCK` + 3 lettres du sujet + numéro de campagne (ex. `#XCKbtc1`), unique à la campagne ; à reporter dans `status.json` (`hashtag`)
- Statut : draft

Notes avant publication :
- <points à revérifier le jour de la publication>

## Fichiers

- `posts.json` : les posts de la campagne, dans l'ordre de publication. Chaque entrée a `index`, `long`, `tech`, `simple`, `source` et `status`, et parfois `variant` (une 3e version).
- `status.json` : l'avancement de la publication (`status` actif ou inactif, `last_posted_index` et `last_posted_at` du dernier post publié). Format dans le README.
