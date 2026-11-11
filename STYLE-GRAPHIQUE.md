# Style graphique @xcryptokeys

Référence : [design/reference-banner.png](design/reference-banner.png) (la bannière du compte).
Exemples : [threads/pactus-001/post_06.png](threads/pactus-001/post_06.png) (schéma sur mesure), [threads/pactus-001/post_01.png](threads/pactus-001/post_01.png) (carte simple).

Le but : qu'on reconnaisse un visuel @xcryptokeys au premier coup d'œil dans le fil X.
Ambiance « réseau la nuit » : fond bleu nuit profond, traits néon cyan → violet, effet verre, pistes de circuit et bribes de code en fond.

## Palette

| Rôle | Couleur | Usage |
|---|---|---|
| Fond, bord | `#040823` | coins de l'image, fond le plus sombre |
| Fond, milieu | `#081039` | dégradé radial du fond |
| Fond, centre | `#10235b` | centre lumineux du fond |
| Panneau | `#0b2053` / `#172667` | cartes « verre » (opacité 70–85 %) |
| Bleu circuit | `#2169af` | bordures fines, grille, pistes |
| Bleu clair | `#488fdb` / `#46b6e9` | libellés secondaires, signature |
| **Cyan néon** | `#5cfcfa` | accent principal : flux, numéro, chiffre clé |
| Indigo | `#4a7ce1` / `#8565e4` | halos, accents secondaires |
| **Violet néon** | `#ad59e9` | accent secondaire : cas optionnel, alerte, rejet |
| Texte | `#eaf6ff` | titres et texte principal |
| Texte atténué | `#8fa8dd` | sous-titres, précisions |

D'autres couleurs sont permises si elles restent dans cette famille froide (bleu, cyan, indigo, violet, magenta).
Pas de rouge vif, d'orange ni de vert saturé : une alerte se fait en violet néon, un « OK » en cyan.
Un seul dégradé d'accent : cyan `#5cfcfa` → violet `#ad59e9`, de gauche à droite.

## Typographie

- **Titres** : Exo 2, 800 (Google Fonts). Une ligne, 40–46 px, couleur `#eaf6ff`.
- **Préfixe de série** (« INSIDE #PACTUS #6 ») : JetBrains Mono, gras, majuscules, espacement 2, cyan néon avec halo.
- **Code, champs, adresses, petits libellés** : JetBrains Mono.
- **Texte courant** : Inter.

## Composition (1200 × 675, exporté en ×2)

1. En haut à gauche : préfixe de série, puis titre (marge 60 px).
2. Au centre : un seul schéma, lisible sur mobile. 3 à 6 boîtes maximum, texte ≥ 15 px.
3. En bas à droite : la signature `@xcryptokeys`, seule (pas de hashtag ni d'autre mention), en mono bleu clair.
4. Fond : dégradé radial + grille 40 px très discrète + 1 ou 2 halos flous (bleu, violet) + quelques pistes de circuit et bribes `0x…`/binaire à 20 % d'opacité, jamais derrière du texte.

## Éléments

- **Cartes** : coins arrondis 12–16 px, fond verre, bordure 1,5 px `#2169af`.
- **Carte mise en avant** (l'idée clé du post) : bordure 2,5 px en dégradé cyan → violet, avec halo.
- **Flèches et flux** : courbes de Bézier, 2,5 px, cyan avec halo. En pointillé violet pour un cas optionnel.
- **Chiffre ou formule clé** : dans une carte mise en avant, en Exo 2 800.
- Pas de logo de marque, de photo ni d'image IA dans les schémas. Les logos de chaînes restent pour la bannière.

## Exactitude

L'image ne doit rien affirmer de plus que le post (`long`, `tech`, `simple`). Un schéma se vérifie comme un post, contre la même source.

## Fichiers

- Chaque post a une image : `threads/<campagne>/post_NN.png` (NN sur 2 chiffres, 3 au-delà de 99).
- Sa source : `threads/<campagne>/src/post_NN.html`.
- **Carte simple** (par défaut) : générée depuis le champ `card` du post (`title`, `sub`, `big` optionnel) :
  `python3 design/card.py threads/<campagne> [index ...] --png`
- **Schéma sur mesure** (quand un dessin explique mieux) : écrire `src/post_NN.html` à la main en partant de `threads/pactus-001/src/post_06.html`, puis
  `node design/render.js threads/<campagne>/src/post_NN.html`.
  card.py ne l'écrase jamais.
- Rendu : Playwright + Chromium (`npm i playwright`).
