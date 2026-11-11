# Les 10 sujets de threads X (crypto en général)

Objectif : rédiger les 10 campagnes, chacune avec au moins 30 posts selon les règles de `STYLE.md` : en anglais, moins de 280 caractères, une info technique par post, au moins 2 versions par post (simple et tech), un préfixe propre au thread, affirmations vérifiées. La campagne Pactus (`threads/pactus-001/`) sert de référence.

Statut : les **10 sujets sont retenus**, sujets et préfixes validés. Rédaction à venir.

Les exemples ci-dessous viennent de ma connaissance du domaine. Chiffres et dates seront revérifiés un par un au moment de la rédaction, comme pour la série Pactus.

---

## 1. Bitcoin sous le capot
**Titre :** "Bitcoin, line by line"  
**Préfixe :** `#Bitcoin Internals #N:`  
**Hashtags fixes :** aucun (avant : #BTC #Crypto)  
**Angle :** les règles précises (et les bizarreries) du protocole que la plupart des holders ignorent.

Exemples :
- La difficulté s'ajuste tous les 2016 blocs, d'un facteur 4 au maximum. Le calcul mesure 2015 intervalles au lieu de 2016 : c'est le bug du "time warp".
- L'offre maximale n'est pas 21 M mais environ 20 999 999,9769 BTC, à cause des arrondis au satoshi à chaque halving.
- Les 50 BTC du bloc genesis ne sont pas dépensables : la transaction n'est jamais entrée dans la base des UTXO.
- Août 2010, bloc 74 638 : un débordement d'entier crée environ 184 milliards de BTC. Corrigé par un soft fork en quelques heures.
- Deux paires de coinbase avec le même txid (blocs 91 812/91 842 et 91 722/91 880) ont mené à BIP30.
- Une récompense de bloc n'est dépensable qu'après 100 confirmations (coinbase maturity).
- L'horodatage d'un bloc doit dépasser la médiane des 11 blocs précédents et ne pas avoir plus de 2 h d'avance.
- Depuis SegWit, la limite est de 4 000 000 unités de poids, et non plus 1 Mo.
- Taproot (Schnorr, BIP340-342) s'active au bloc 709 632, le 14 novembre 2021.
- Bitcoin n'a pas de comptes : un "solde" est la somme des UTXO que ta clé peut débloquer.

**Pourquoi 30+ :** il y a des dizaines de règles de consensus, d'incidents historiques et de BIP, chacun donne un post autonome. Très large audience.

## 2. Ethereum sous le capot
**Titre :** "How Ethereum actually works"  
**Préfixe :** `#Ethereum Deep Dive #N:`  
**Hashtags fixes :** aucun (avant : #ETH #SmartContracts)  
**Angle :** du modèle de comptes au Merge et aux blobs, les mécanismes concrets.

Exemples :
- EIP-1559 (août 2021) : la base fee est brûlée et varie d'au plus 12,5 % par bloc selon le remplissage par rapport à la cible.
- Une adresse = les 20 derniers octets du Keccak-256 de la clé publique. La casse mixte (EIP-55) sert de checksum.
- Ethereum utilise Keccak-256, pas SHA3-256 du NIST : le padding diffère, donc les hash aussi.
- Le Merge (15 septembre 2022) s'est déclenché sur une "terminal total difficulty", pas sur une hauteur de bloc.
- Slot de 12 s, epoch de 32 slots (6,4 min). Un bloc est finalisé après environ 2 epochs.
- Dencun (mars 2024) introduit les blobs : environ 128 Ko chacun, supprimés par les nœuds après environ 18 jours.
- EIP-6780 (Dencun) neutralise SELFDESTRUCT, sauf dans la transaction qui crée le contrat.
- Pectra (mai 2025) : un validateur peut détenir jusqu'à 2048 ETH effectifs (EIP-7251), au lieu de 32.
- L'adresse d'un contrat se calcule à l'avance : CREATE dépend de l'émetteur et de son nonce, CREATE2 d'un salt et du code.
- Fusaka (décembre 2025) apporte PeerDAS : les nœuds vérifient les blobs par échantillonnage sans tout télécharger.

**Pourquoi 30+ :** chaque hard fork (Homestead à Fusaka) apporte plusieurs EIP intéressantes, en plus de l'EVM, du gas et du staking.

## 3. La cryptographie des cryptos
**Titre :** "The math that guards your coins"  
**Préfixe :** `#Crypto Math #N:`  
**Hashtags fixes :** aucun (avant : #Cryptography #Blockchain)  
**Angle :** signatures, hash et seed phrases expliqués par leurs détails, avec les échecs célèbres.

Exemples :
- Bitcoin et Ethereum utilisent la courbe secp256k1, une courbe de Koblitz, pas les courbes NIST.
- Réutiliser le nonce d'une signature ECDSA révèle la clé privée. C'est ainsi que la PS3 a été cassée en 2010.
- En 2013, un bug du générateur aléatoire d'Android a permis de voler des BTC dans des wallets mobiles via des nonces répétés.
- Une seed de 12 mots = 128 bits d'entropie + 4 bits de checksum. Le dernier mot est en partie un checksum.
- La liste BIP39 compte 2048 mots, soit 11 bits par mot.
- Les signatures Schnorr sont linéaires : plusieurs clés peuvent produire une signature unique (MuSig2), indiscernable d'une signature simple.
- Les validateurs Ethereum signent en BLS12-381 : des milliers de signatures s'agrègent en une seule.
- Bech32 avait une faiblesse (ajouter ou retirer des "q" avant un "p" final). D'où Bech32m (BIP350) pour Taproot.
- Une adresse Bitcoin classique = RIPEMD-160(SHA-256(clé publique)).
- L'algorithme de Shor menacerait ECDSA. Les adresses dont la clé publique est déjà sur la chaîne sont les plus exposées.

**Pourquoi 30+ :** chaque primitive (hash, ECDSA, Schnorr, BLS, Merkle, ZK, encodages) a son mécanisme et son incident réel.

## 4. Les grands hacks, expliqués techniquement
**Titre :** "Anatomy of a crypto heist"  
**Préfixe :** `Crypto #Hack Autopsy #N:`  
**Hashtags fixes :** aucun (avant : #CyberSecurity #InfoSec)  
**Angle :** pas le montant volé, mais la ligne de code ou la clé qui a lâché. C'est le sujet le plus viral.

Exemples :
- The DAO (2016) : une réentrance vide environ 3,6 M ETH. Le hard fork qui suit crée Ethereum Classic.
- Parity (novembre 2017) : un utilisateur devient propriétaire de la bibliothèque partagée puis la détruit. Environ 513 000 ETH gelés à jamais.
- Ronin (mars 2022) : 5 clés de validateurs sur 9 compromises, environ 625 M$. Le vol est découvert 6 jours après.
- Wormhole (février 2022) : une vérification de signature contournée sur Solana permet de créer 120 000 wETH sans dépôt.
- Nomad (août 2022) : une mise à jour rend valide n'importe quel message. Des centaines d'adresses copient la transaction de l'attaquant.
- Beanstalk (avril 2022) : un flash loan achète la majorité des votes et fait passer une proposition qui vide le protocole (environ 182 M$).
- Curve (juillet 2023) : un bug du compilateur Vyper (0.2.15 à 0.3.0) casse le verrou anti-réentrance de plusieurs pools.
- Bybit (février 2025) : environ 1,5 Md$ en ETH. L'interface Safe compromise affiche une transaction qui n'est pas celle signée.
- Cetus (mai 2025, Sui) : une vérification de débordement erronée dans une bibliothèque mathématique, environ 223 M$.
- Poly Network (août 2021) : environ 611 M$ volés, puis presque tout rendu par l'attaquant.

**Pourquoi 30+ :** il y a facilement 30 hacks majeurs, chacun avec une cause technique précise. On peut même faire 2 posts par hack.

## 5. Le consensus, comment des inconnus se mettent d'accord
**Titre :** "How strangers agree: consensus 101 to 401"  
**Préfixe :** `#Blockchain Consensus #N:`  
**Hashtags fixes :** aucun (avant : #ProofOfStake #ProofOfWork)  
**Angle :** PoW, PoS, BFT et leurs compromis, avec des attaques réelles.

Exemples :
- Les protocoles BFT tolèrent f nœuds malveillants si n ≥ 3f + 1, d'où le seuil de 2/3.
- Le résultat FLP (1985) : aucun protocole déterministe ne garantit le consensus en réseau asynchrone avec une seule panne possible.
- Selfish mining (Eyal et Sirer, 2013) : un mineur peut gagner plus que sa part avec moins de 50 % du hashrate.
- Les "6 confirmations" de Bitcoin sont une convention : le whitepaper calcule 5 blocs pour un attaquant à 10 % du hashrate.
- Ethereum Casper FFG : un checkpoint est finalisé quand 2/3 du stake vote deux epochs de suite.
- Si plus d'1/3 des validateurs Ethereum disparaissent, l'"inactivity leak" réduit leur stake jusqu'à retrouver 2/3.
- Les chaînes Tendermint/CometBFT s'arrêtent plutôt que de forker si plus d'1/3 du stake est hors ligne.
- Ethereum Classic a subi plusieurs attaques à 51 % (2019, 2020) avec réorganisation de milliers de blocs.
- Proof of History de Solana est une chaîne de SHA-256 séquentiels qui sert d'horloge, pas un mécanisme de consensus en soi.
- "Weak subjectivity" : un nouveau nœud PoS doit partir d'un checkpoint récent de confiance, à cause des attaques longue portée.

**Pourquoi 30+ :** chaque famille (Nakamoto, BFT, Casper, Avalanche, Algorand, DAG) donne 4 à 6 posts, plus les attaques.

## 6. Le scaling : Lightning, rollups et L2
**Titre :** "Scaling without breaking: L2s explained"  
**Préfixe :** `Crypto #Layer2 Unlocked #N:`  
**Hashtags fixes :** aucun (avant : #Rollups #Lightning)  
**Angle :** ce que garantit vraiment un L2 et ce qu'il ne garantit pas.

Exemples :
- Un canal Lightning est un multisig 2-sur-2. Diffuser un ancien état permet à l'autre partie de tout prendre (pénalité).
- Les paiements Lightning multi-sauts reposent sur les HTLC : un même hash verrouille chaque saut.
- Le routage Lightning est en oignon (Sphinx) : chaque nœud ne connaît que le saut précédent et le suivant.
- Les rollups optimistes laissent environ 7 jours pour contester un état. D'où les retraits lents.
- Une contestation Arbitrum se règle par dichotomie jusqu'à une seule instruction, rejouée sur L1.
- Les ZK rollups publient une preuve de validité : pas de délai de contestation.
- La plupart des rollups ont un seul séquenceur. Il peut retarder une transaction, mais il existe une inclusion forcée via L1.
- L2BEAT classe les rollups en "stages" 0, 1 et 2 selon le pouvoir restant aux clés d'administration.
- Après Dencun (blobs), les frais de nombreux L2 ont baissé fortement.
- Une sidechain (validateurs propres) n'est pas un rollup : sa sécurité ne vient pas du L1.

**Pourquoi 30+ :** Lightning seul tient 12 à 15 posts, rollups optimistes et ZK autant. Sujet au cœur des débats actuels.

## 7. MEV et mempool : la forêt sombre
**Titre :** "The dark forest: MEV explained"  
**Préfixe :** `Crypto #MEV Dark Forest #N:`  
**Hashtags fixes :** aucun (avant : #DeFi #CryptoTrading)  
**Angle :** ce qui arrive à ta transaction entre "Envoyer" et l'inclusion dans un bloc.

Exemples :
- Le terme MEV vient de l'article "Flash Boys 2.0" (Daian et al., 2019).
- Un sandwich : un bot achète avant toi, ta transaction fait monter le prix, il revend juste après.
- "Ethereum is a Dark Forest" (août 2020) : des fonds récupérables ont été pris par un bot généraliste qui copiait la transaction.
- Après le Merge, la grande majorité des blocs Ethereum passent par MEV-Boost : des builders les construisent, le proposer choisit l'offre.
- Quelques builders produisent la plupart des blocs Ethereum, une nouvelle forme de centralisation.
- Les RPC privés (type Flashbots Protect) évitent la mempool publique, et donc les sandwichs.
- Les liquidations DeFi et les arbitrages entre DEX sont du MEV "utile" qui maintient les prix alignés.
- Time-bandit : si le MEV d'un bloc passé est assez gros, réécrire l'histoire peut devenir rentable.
- Bitcoin Core 28 active par défaut le full-RBF : toute transaction non confirmée peut être remplacée avec plus de frais.
- Sur Solana, il n'y a pas de mempool publique classique : des enchères de bundles (Jito) jouent ce rôle.

**Pourquoi 30+ :** les types de MEV, l'architecture PBS, les incidents et les protections donnent beaucoup de matière. Sujet qui touche directement les utilisateurs.

## 8. La mécanique de la DeFi
**Titre :** "DeFi mechanics, formula by formula"  
**Préfixe :** `#DeFi Mechanics #N:`  
**Hashtags fixes :** aucun (avant : #Stablecoins #Web3 #DEX)  
**Angle :** les formules derrière les DEX, prêts et stablecoins.

Exemples :
- Uniswap v2 : x · y = k. Chaque swap garde le produit des réserves constant (hors frais de 0,3 %).
- Uniswap v3 : la liquidité est concentrée dans des "ticks", chaque tick valant un facteur de prix de 1,0001.
- Uniswap v4 (janvier 2025) : un seul contrat pour toutes les pools et des "hooks" personnalisables.
- Impermanent loss : si le prix double, un LP v2 perd environ 5,7 % par rapport à un simple holding.
- Un flash loan doit être remboursé dans la même transaction, sinon tout est annulé.
- Aave : une position est liquidable quand son "health factor" passe sous 1.
- Black Thursday (mars 2020) : des enchères de liquidation Maker ont été gagnées à 0 DAI à cause de la congestion.
- Lire un prix spot d'un DEX comme oracle se manipule en une transaction. D'où les TWAP et les oracles externes.
- Curve StableSwap mélange somme constante et produit constant pour des stablecoins quasi au même prix.
- Attaque par inflation ERC-4626 : le premier déposant d'un vault peut voler le dépôt suivant par arrondi.

**Pourquoi 30+ :** chaque primitive (AMM, prêt, stablecoin, oracle, vault, perpétuels) a ses formules et ses cas limites.

## 9. Wallets et self-custody
**Titre :** "Not your keys: wallet security deep dive"  
**Préfixe :** `Crypto #Wallet Security #N:`  
**Hashtags fixes :** aucun (avant : #CryptoSecurity #SelfCustody)  
**Angle :** ce que fait réellement un wallet, et comment les gens se font vider.

Exemples :
- Avec une xpub et une seule clé privée enfant non durcie, on peut retrouver la clé privée parente (BIP32).
- Une passphrase BIP39 crée un wallet totalement différent. Il n'existe pas de "mauvaise" passphrase, juste un autre wallet vide.
- SLIP-39 (Shamir) : la seed est divisée en parts, par exemple 3 sur 5 suffisent pour la reconstituer.
- Address poisoning : un attaquant envoie un transfert depuis une adresse aux mêmes premiers et derniers caractères que ton destinataire habituel.
- Une signature Permit ne coûte pas de gas et n'apparaît pas on-chain, mais elle peut autoriser un drainer à tout prendre.
- Blind signing : le wallet matériel signe un hash qu'il ne peut pas décoder. Bybit en est l'exemple extrême.
- EIP-712 affiche des données typées lisibles au lieu d'un hash opaque.
- ERC-4337 (2023) : l'account abstraction sans changer le protocole, via un contrat EntryPoint.
- EIP-7702 (Pectra) : un compte classique peut déléguer à du code de contrat. C'est utile, et c'est aussi un nouveau vecteur de phishing.
- Avec Taproot et MuSig2, un multisig ressemble on-chain à une signature simple.

**Pourquoi 30+ :** sujet très utile pour tous les profils, avec beaucoup d'arnaques réelles à décortiquer. Bon pour fidéliser.

## 10. L'anonymat dans la crypto
**Titre :** "Is crypto really anonymous?"  
**Préfixe :** `Crypto #Anonymity Myths #N:`  
**Hashtags fixes :** aucun (avant : #CryptoPrivacy #Monero)  
**Angle :** casser le mythe "Bitcoin est anonyme" : la blockchain est pseudonyme, comment on remonte jusqu'aux gens, et ce qui rend vraiment anonyme.

Exemples :
- Bitcoin n'est pas anonyme mais pseudonyme : chaque transaction est publique pour toujours, seule l'identité derrière l'adresse est cachée.
- Le whitepaper Bitcoin (section 10) prévient déjà : les entrées d'une même transaction trahissent un même propriétaire.
- Cette heuristique de propriété commune des entrées est la base de la chain analysis.
- Hack de Bitfinex (2016, environ 120 000 BTC) : les enquêteurs ont suivi les fonds pendant des années, arrestations en 2022 et environ 94 000 BTC saisis.
- Colonial Pipeline (2021) : le DOJ a récupéré environ 63,7 BTC de la rançon en suivant les fonds jusqu'à un wallet.
- Il suffit d'un passage par un exchange avec KYC pour relier une adresse à une identité.
- CoinJoin (Greg Maxwell, 2013) casse l'heuristique en mélangeant les entrées de plusieurs personnes.
- Monero cache l'émetteur dans un anneau de 16 signataires possibles et utilise des adresses à usage unique.
- Zcash permet des transactions entièrement chiffrées grâce aux zk-SNARKs, mais la plupart des transactions restent transparentes.
- Tornado Cash : sanctionné par l'OFAC en août 2022, puis retiré de la liste en mars 2025 après une décision de justice sur les contrats immuables.

**Pourquoi 30+ :** le mythe, les méthodes de traçage, les grandes affaires résolues par la blockchain, puis les outils (CoinJoin, Monero, Zcash, mixers) et le volet juridique. Sujet qui fait réagir.

---

## Ordre de rédaction proposé

1. **Grands hacks (4)** : le plus partageable, chaque post est une histoire. Bon thread de lancement.
2. **Bitcoin sous le capot (1)** : audience la plus large.
3. **Ethereum sous le capot (2)** : deuxième audience, et beaucoup d'actualité récente.
4. **MEV (7)** : fort potentiel de "je ne savais pas".
5. **Wallets et self-custody (9)** : utile, sauvegardé et partagé.
6. **Anonymat (10)**, 7. **DeFi (8)**, 8. **Cryptographie (3)**, 9. **Scaling (6)**, 10. **Consensus (5)**.

En réserve, si un sujet ne fonctionne pas : l'histoire de la crypto en moments techniques (Hashcash, b-money, bloc genesis, pizza, block size wars) et une comparaison des architectures de chaînes (Solana, Cosmos, Polkadot, Cardano, Move).

---

## Préfixes et hashtags validés

Règles : préfixe court, propre au sujet, qui montre qu'on parle de crypto, avec au moins un hashtag très utilisé. **2 hashtags maximum par post** : celui du préfixe (pour être trouvé en recherche) et le hashtag de campagne que le programme ajoute à la fin (`#XCKbtc1`…, pour retrouver toute la série). Plus de hashtags fixes en fin de texte : la colonne garde ceux d'avant pour mémoire.

| # | Sujet | Préfixe | Hashtags fixes |
|---|---|---|---|
| 1 | Bitcoin sous le capot | `#Bitcoin Internals #N:` | aucun (avant : #BTC #Crypto) |
| 2 | Ethereum sous le capot | `#Ethereum Deep Dive #N:` | aucun (avant : #ETH #SmartContracts) |
| 3 | La cryptographie des cryptos | `#Crypto Math #N:` | aucun (avant : #Cryptography #Blockchain) |
| 4 | Les grands hacks | `Crypto #Hack Autopsy #N:` | aucun (avant : #CyberSecurity #InfoSec) |
| 5 | Le consensus | `#Blockchain Consensus #N:` | aucun (avant : #ProofOfStake #ProofOfWork) |
| 6 | Le scaling, L2 | `Crypto #Layer2 Unlocked #N:` | aucun (avant : #Rollups #Lightning) |
| 7 | MEV et mempool | `Crypto #MEV Dark Forest #N:` | aucun (avant : #DeFi #CryptoTrading) |
| 8 | La mécanique de la DeFi | `#DeFi Mechanics #N:` | aucun (avant : #Stablecoins #Web3 #DEX) |
| 9 | Wallets et self-custody | `Crypto #Wallet Security #N:` | aucun (avant : #CryptoSecurity #SelfCustody) |
| 10 | L'anonymat dans la crypto | `Crypto #Anonymity Myths #N:` | aucun (avant : #CryptoPrivacy #Monero) |
