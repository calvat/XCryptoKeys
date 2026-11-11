# XCryptoKeys

Posts et threads préparés pour le compte X **@xcryptokeys** : blockchains et crypto en général.

## Organisation

| Fichier / dossier | Contenu |
|---|---|
| `SUJETS.md` | Les 10 sujets, leurs préfixes, leurs hashtags fixes et l'ordre de rédaction |
| `STYLE.md` | Les règles de rédaction de chaque post |
| `STYLE-GRAPHIQUE.md` | Le style des illustrations (palette, polices, composition) |
| `design/` | Bannière de référence et script de rendu PNG |
| `threads/` | Un dossier par campagne, nommé `<nom technique>-<numéro sur 3 chiffres>` : par exemple `threads/pactus-001/` pour la 1re campagne Pactus. Il contient `index.md` (description de la campagne), `posts.json` (les posts) et `status.json` (où en est la publication) |
| `threads/_template/` | Le modèle à copier pour une nouvelle campagne |
| `src/` | Le programme C# qui publie automatiquement les posts sur X (voir [`src/README.md`](src/README.md)) |

## Règles clés (détails dans `STYLE.md`)

- **30 à 100 posts** par thread, **1 post par thread et par jour**, en **anglais**, **moins de 280 caractères**, **une info technique** par post.
- Technique mais compréhensible par un non-développeur.
- **2 versions minimum par post** (a simple, b tech, parfois c) : Johan choisit au moment de publier.
- Un **préfixe par thread**, identique sur tous ses posts, qui montre qu'on parle de crypto et contient un hashtag très utilisé.
- **1 à 3 hashtags fixes par série**, très utilisés, dans tous ses posts.

## Format de `posts.json`

Un tableau, dans l'ordre de publication. Chaque entrée :

```json
{
  "index": 1,
  "long": "ce qu'on veut dire, 500 caractères max (écrit en premier)",
  "tech": "version technique, tirée de long (écrite en 2e)",
  "simple": "version simple, préfixe et hashtags compris (écrite en dernier)",
  "source": "URL lue pour vérifier",
  "status": "checked"
}
```

Quelques posts ont aussi `variant`, une 3e version.

## Format de `status.json`

Un objet par campagne, qui dit où en est la publication :

```json
{
  "status": "inactive",
  "last_posted_index": 0,
  "last_posted_at": null,
  "interval_days": 1,
  "post_time": "09:00",
  "hashtag": "#XCKbtc1"
}
```

- `status` : `active` (campagne en cours de publication) ou `inactive` (pas commencée, en pause ou terminée)
- `last_posted_index` : `index` (dans `posts.json`) du dernier post publié ; `0` si rien n'est encore publié. Le prochain post à publier est donc `last_posted_index + 1`
- `last_posted_at` : date du dernier post publié, au format ISO 8601 (`2026-11-11` ou `2026-11-11T09:30:00Z`) ; `null` si rien n'est encore publié
- `interval_days` : nombre de jours entre deux posts (`1` = un post par jour, la règle par défaut)
- `post_time` : heure de publication, au format `HH:MM` en UTC
- `hashtag` : hashtag unique de la campagne, `#XCK` + 3 lettres du sujet + numéro de campagne (ex. `#XCKbtc1`). Le programme l'ajoute à la fin de chaque post, et il compte dans les 280 caractères. Il est aussi indiqué dans `index.md`

Le prochain post (`last_posted_index + 1`) est dû le jour `last_posted_at + interval_days`, à l'heure `post_time`. Pour le premier post d'une campagne, on la passe en `active` et on publie le jour choisi.

À chaque publication, mettre à jour `last_posted_index` et `last_posted_at` ensemble (et passer le post en `posted` dans `posts.json`). Le programme de `src/` le fait lui-même, puis commit et push.

## Statut d'un post

Champ `status` de chaque post :

- `draft` : rédigé, pas encore vérifié
- `checked` : chaque affirmation a été vérifiée sur une source
- `posted` : publié sur X (ajouter la date et le lien)

Un thread ne se publie que quand tous ses posts sont `checked`. Les notes en tête de chaque fichier listent les points à revérifier le jour de la publication (affaires en cours, mises à jour de réseau).

## Threads

| Campagne | Thème | Posts | Statut |
|---|---|---|---|
| pactus-001 | [Inside Pactus](threads/pactus-001/index.md) | 90 (versions simple et tech) | checked |
| bitcoin-001 | [Bitcoin sous le capot](threads/bitcoin-001/index.md) · `#Bitcoin Internals` | 71 | checked |
| ethereum-001 | [Ethereum sous le capot](threads/ethereum-001/index.md) · `#Ethereum Deep Dive` | 91 | checked |
| crypto-math-001 | [La cryptographie des cryptos](threads/crypto-math-001/index.md) · `#Crypto Math` | 76 | checked |
| hacks-001 | [Les grands hacks](threads/hacks-001/index.md) · `Crypto #Hack Autopsy` | 52 | checked |
| consensus-001 | [Le consensus](threads/consensus-001/index.md) · `#Blockchain Consensus` | 54 | checked |
| layer2-001 | [Le scaling, L2](threads/layer2-001/index.md) · `Crypto #Layer2 Unlocked` | 67 | checked |
| mev-001 | [MEV et mempool](threads/mev-001/index.md) · `Crypto #MEV Dark Forest` | 52 | checked |
| defi-001 | [La mécanique de la DeFi](threads/defi-001/index.md) · `#DeFi Mechanics` | 53 | checked |
| wallets-001 | [Wallets et self-custody](threads/wallets-001/index.md) · `Crypto #Wallet Security` | 67 | checked |
| anonymity-001 | [L'anonymat dans la crypto](threads/anonymity-001/index.md) · `Crypto #Anonymity Myths` | 67 | checked |
