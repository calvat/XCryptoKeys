# Campagne defi-001 : DeFi mechanics, formula by formula

- Sujet : 8. La mécanique de la DeFi
- Angle : les formules derrière les DEX, les prêts, les stablecoins, les oracles et les vaults, avec leurs cas limites et les incidents qui les ont révélées.
- Préfixe : `#DeFi Mechanics #N:`
- Hashtags fixes : aucun (2 hashtags maximum par post : celui du préfixe et celui de la campagne)
- Hashtag de campagne : `#XCKdfi1`, unique à la campagne, ajouté par le programme à la fin de chaque post (compte dans les 280 caractères)
- Statut : checked (53 posts checked, 0 draft)

Notes avant publication :
- "checked" = vérifié contre la source indiquée (whitepaper, doc officielle, post-mortem) ou calcul direct à partir de la formule citée. "draft" = fait connu mais non revérifié pendant la rédaction (budget de recherche épuisé) : à recontrôler avant de publier.
- Les pourcentages d'impermanent loss et les exemples chiffrés (health factor, impact de prix) sont des calculs à partir des formules, arrondis.
- Post 33 (close factor Aave) : règle de la v3.3, peut changer par gouvernance.

## Fichiers

- `posts.json` : les 53 posts de la campagne, dans l'ordre de publication. Chaque entrée a `index`, `simple`, `tech`, `source` et `status`, et parfois `variant` (une 3e version).
- `status.json` : l'avancement de la publication (`status` actif ou inactif, `last_posted_index` et `last_posted_at` du dernier post publié). Format dans le README.
