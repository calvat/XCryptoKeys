# Campagne crypto-math-001 : The math that guards your coins

- Sujet : 3. La cryptographie des cryptos
- Angle : signatures, hash et seed phrases expliqués par leurs détails, avec les échecs célèbres.
- Préfixe : `#Crypto Math #N:`
- Hashtags fixes : aucun (2 hashtags maximum par post : celui du préfixe et celui de la campagne)
- Hashtag de campagne : `#XCKcry1`, unique à la campagne, ajouté par le programme à la fin de chaque post (compte dans les 280 caractères)
- Statut : checked (76 posts checked, 0 draft)

Notes :
- Longueurs comptées comme X (caractères hors latin comptés 2). Toutes les versions font moins de 280.
- Sources : BIPs, EIPs et code source (GitHub) en priorité, puis post-mortems et presse pour les incidents.
- Post 9 : la formule de récupération de clé a aussi été vérifiée par calcul (script Python sur secp256k1).
- Post 75 (BIP360, aussi cité aux posts 72-73) décrit un brouillon qui change souvent de nom : revérifier son statut avant de publier.
- Les montants volés (posts 60, 63) sont en dollars au moment des faits.

## Fichiers

- `posts.json` : les 76 posts de la campagne, dans l'ordre de publication. Chaque entrée a `index`, `simple`, `tech`, `source` et `status`, et parfois `variant` (une 3e version).
- `status.json` : l'avancement de la publication (`status` actif ou inactif, `last_posted_index` et `last_posted_at` du dernier post publié). Format dans le README.
