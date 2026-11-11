# Campagne pactus-001 : Inside Pactus

- Sujet : Pactus (série de lancement du compte)
- Angle : comment Pactus fonctionne, vérifié dans le code de `main`
- Préfixe : `Inside #Pactus #N:`
- Hashtags fixes : #PAC @PactusChain
- Statut : checked (vérifiés contre le code), à publier

English, 280 characters max, one technical point per post. Each post has two versions: "Na (simple)" for a general audience and "Nb (tech)". Pick one per number. "≈" values are arithmetic from the constants (block time = 10 s). Figures are protocol defaults, which match the mainnet genesis unless a post says otherwise.

Things to know before posting:
- Series 5 (consensus): cross-check before posting.
- Post 56 (anchors, PIP-50) needs protocol v5, which activates by a 75 % stake signal. Check that it is active on mainnet before posting it as a live feature.
- Post 3: rewards are paid from the 21M PAC treasury, so at the coded rates it is empty around block 44M. The 0.25 tier never reaches 56M and the 0.125 tier is never reached. Don't add "then 0.125".

## Séries

- Series 1 — Chain basics & economics : posts 1 à 9
- Series 2 — Block & certificate format : posts 10 à 20
- Series 3 — Committee & sortition : posts 21 à 30
- Series 4 — Staking : posts 31 à 37
- Series 5 — Consensus ⚠️ : posts 38 à 47
- Series 6 — Crypto & keys : posts 48 à 56
- Series 7 — Transactions & mempool : posts 57 à 63
- Series 8 — State & storage : posts 64 à 69
- Series 9 — Networking & sync : posts 70 à 80
- Series 10 — Node, API & tooling : posts 81 à 90

## Fichiers

- `posts.json` : les 90 posts de la campagne, dans l'ordre de publication. Chaque entrée a `index`, `simple`, `tech`, `source` et `status`.
