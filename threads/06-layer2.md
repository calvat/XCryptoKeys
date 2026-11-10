# Thread : Scaling without breaking: L2s explained

- Sujet : 6. Le scaling : Lightning, rollups et L2 (SUJETS.md)
- Angle : ce que garantit vraiment un L2 et ce qu'il ne garantit pas.
- Préfixe : `Crypto #Layer2 Unlocked #N:`
- Hashtags fixes : #Rollups #Lightning
- Statut : tous les posts vérifiés (revérification des chiffres sensibles au temps : 10 nov. 2026)

Notes avant publication :
- Comptage X : chaque version fait moins de 280 caractères, préfixe et hashtags compris (pondération X appliquée : « ≈ » et « → » comptent 2 ; ils sont évités ici).
- Posts 31, 32, 37, 45 : recoupés le 10 nov. 2026 (ethereum.org, glossaire ETHBEAT, spec OP, article Groth16).
- Posts 36, 49, 50 (délais Arbitrum et OP Stack) : paramètres modifiables par la gouvernance. Revérifiés le 10 nov. 2026 (inchangés : 45 818 blocs / 6,4 j, bond 3 600 ETH, force-include 7 200 blocs / 24 h ; OP sequencing window 3 600 blocs L1 = 12 h). Post 56b réécrit sans le délai d'upgrade Arbitrum (proposition DAO de passer le timelock L2 de 3 à 8 jours ; exécution non vérifiée).
- Posts 41, 53 à 55 : L2BEAT s'appelle désormais ETHBEAT (l2beat.com redirige vers ethbeat.org) ; le framework Stages est annoncé inchangé. Au 10 nov. 2026 : Arbitrum One et OP Mainnet en Stage 1 ; Stage 2 exige au moins 30 jours de fenêtre de sortie (55b mis à jour). Revérifier avant de publier.
- Post 64 : au 10 nov. 2026, BPO2 (14/21) reste la dernière limite en vigueur : EIP-8135 (BPO2) est Final, EIP-8138 (BPO3) est un brouillon sans valeurs ni date. Revérifier si BPO3 ou Glamsterdam passent sur mainnet avant publication.

---

### 1/ · checked · choix :

**1a (simple)** : Crypto #Layer2 Unlocked #1: Layer 2s promise cheap, fast transactions on top of Bitcoin and Ethereum. But what do they really guarantee, and what don't they? One fact a day on Lightning, rollups and sidechains. #Rollups #Lightning

**1b (tech)** : Crypto #Layer2 Unlocked #1: Lightning channels, optimistic rollups, ZK rollups: each moves activity off L1 but leans on L1 for security in its own way. This series breaks down who you trust, for what, and how you exit. #Rollups #Lightning

**1c (variante)** : Crypto #Layer2 Unlocked #1: "Layer 2" is a label, not a guarantee. Some L2s let you exit with your funds even if the operator vanishes. Others don't. Learn to tell them apart, one post a day. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52 ; https://github.com/lightning/bolts/blob/master/03-transactions.md

### 2/ · checked · choix :

**2a (simple)** : Crypto #Layer2 Unlocked #2: Every node of a blockchain checks every transaction, so block space is limited on purpose. Layer 2s move most activity elsewhere and use the base chain to settle or resolve disputes. #Rollups #Lightning

**2b (tech)** : Crypto #Layer2 Unlocked #2: Bitcoin caps blocks at 4,000,000 weight units (BIP141) so ordinary nodes can verify everything. L2s keep that cap and move transactions off-chain, touching L1 only for settlement, data or disputes. #Rollups #Lightning

Source : https://github.com/bitcoin/bips/blob/master/bip-0141.mediawiki

### 3/ · checked · choix :

**3a (simple)** : Crypto #Layer2 Unlocked #3: A Lightning channel starts with one Bitcoin transaction that locks coins in a shared vault. Moving them out needs both parties' signatures, so neither side can walk off alone with the money. #Rollups #Lightning

**3b (tech)** : Crypto #Layer2 Unlocked #3: A Lightning channel is funded by an on-chain P2WSH output with the script 2 <pubkey1> <pubkey2> 2 OP_CHECKMULTISIG (BOLT 3): a 2-of-2 multisig. Spending it needs both peers' signatures. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/03-transactions.md

### 4/ · checked · choix :

**4a (simple)** : Crypto #Layer2 Unlocked #4: Inside a Lightning channel, each payment just swaps a new signed "who owns what" transaction between the two partners. Nothing hits the blockchain until the channel closes. #Rollups #Lightning

**4b (tech)** : Crypto #Layer2 Unlocked #4: Each channel update replaces the commitment transaction: a fully signed tx spending the funding output to the latest balances. Each peer holds its own version and broadcasts it only to close alone. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md ; https://github.com/lightning/bolts/blob/master/03-transactions.md

### 5/ · checked · choix :

**5a (simple)** : Crypto #Layer2 Unlocked #5: Lightning punishes cheating hard. If someone publishes an old channel state that gave them more money, the other side can claim the whole channel balance as a penalty. #Rollups #Lightning

**5b (tech)** : Crypto #Layer2 Unlocked #5: Each update revokes the previous commitment by revealing its per-commitment secret. If a peer broadcasts a revoked state, the victim uses the revocation key to sweep every output in a penalty tx. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/03-transactions.md

### 6/ · checked · choix :

**6a (simple)** : Crypto #Layer2 Unlocked #6: When you close a Lightning channel on your own, your coins stay locked for a while. That wait gives your partner time to prove you published an outdated state and punish you. #Rollups #Lightning

**6b (tech)** : Crypto #Layer2 Unlocked #6: In a unilateral close, outputs paying the broadcaster are delayed by to_self_delay blocks, a relative timelock (nSequence/CSV). During that window the counterparty can use the revocation path. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/03-transactions.md

### 7/ · checked · choix :

**7a (simple)** : Crypto #Layer2 Unlocked #7: Each side of a Lightning channel keeps a small deposit it can't spend, about 1% of the channel. That way a would-be cheater always has something to lose. #Rollups #Lightning

**7b (tech)** : Crypto #Layer2 Unlocked #7: BOLT 2 suggests channel_reserve_satoshis of 1% of the channel total: a balance each peer can't spend, so broadcasting a revoked commitment always risks real funds. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md

### 8/ · checked · choix :

**8a (simple)** : Crypto #Layer2 Unlocked #8: Lightning assumes you watch the chain for cheating. If you're offline, a watchtower can do it for you: it holds encrypted penalty transactions and publishes one only if your partner cheats. #Rollups #Lightning

**8b (tech)** : Crypto #Layer2 Unlocked #8: An lnd watchtower stores encrypted justice transactions and can only decrypt and broadcast one after the matching revoked commitment appears on-chain. Altruist towers return all funds minus fees. #Rollups #Lightning

Source : https://github.com/lightningnetwork/lnd/blob/master/docs/watchtower.md

### 9/ · checked · choix :

**9a (simple)** : Crypto #Layer2 Unlocked #9: Lightning lets you pay people you have no channel with, by passing the payment through others. A cryptographic lock makes sure each middleman is paid only if the final recipient is. #Rollups #Lightning

**9b (tech)** : Crypto #Layer2 Unlocked #9: Multi-hop payments use HTLCs: every hop locks funds to the same payment_hash = SHA256(preimage). The payee reveals the preimage to claim, and it flows back hop by hop, settling each HTLC. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md

### 10/ · checked · choix :

**10a (simple)** : Crypto #Layer2 Unlocked #10: Each hop of a Lightning payment has a deadline, and deadlines get shorter toward the recipient. If a payment fails, money is refunded hop by hop and no middleman is left out of pocket. #Rollups #Lightning

**10b (tech)** : Crypto #Layer2 Unlocked #10: Each hop sets the outgoing HTLC's cltv_expiry below the incoming one by its cltv_expiry_delta. That margin lets a node learn the preimage downstream and claim upstream before its own HTLC expires. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md ; https://github.com/lightning/bolts/blob/master/07-routing-gossip.md

### 11/ · checked · choix :

**11a (simple)** : Crypto #Layer2 Unlocked #11: Lightning routes payments in layers, like Tor. Each node on the path only knows who handed it the payment and who to pass it to next, not who sent it or who receives it in the end. #Rollups #Lightning

**11b (tech)** : Crypto #Layer2 Unlocked #11: Lightning uses a Sphinx-based onion (BOLT 4). Each hop peels one layer encrypted to its key and learns only the previous and next hop, not the route length or its own position in it. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/04-onion-routing.md

### 12/ · checked · choix :

**12a (simple)** : Crypto #Layer2 Unlocked #12: Every Lightning onion packet has the same size, however many hops are left. A node in the middle can't tell if it's the first hop, the last one, or somewhere in between. #Rollups #Lightning

**12b (tech)** : Crypto #Layer2 Unlocked #12: A BOLT 4 onion packet is always 1366 bytes, with 1300 bytes of hop_payloads. Each hop removes its own data and refills the end with pseudo-random bytes, so the packet never shrinks. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/04-onion-routing.md

### 13/ · checked · choix :

**13a (simple)** : Crypto #Layer2 Unlocked #13: In Lightning, the sender picks the whole route. Nodes share a public map of channels, and your wallet chooses a path with enough capacity and low fees. #Rollups #Lightning

**13b (tech)** : Crypto #Layer2 Unlocked #13: Lightning uses source routing: the sender builds the path from the gossip graph (BOLT 7 channel_announcement and channel_update). Each announced channel points to a real on-chain funding tx via short_channel_id. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/07-routing-gossip.md

### 14/ · checked · choix :

**14a (simple)** : Crypto #Layer2 Unlocked #14: Lightning routing nodes charge small fees to forward payments: a fixed part plus a part that grows with the amount. Each node sets them per channel and announces them to the network. #Rollups #Lightning

**14b (tech)** : Crypto #Layer2 Unlocked #14: Each channel_update carries fee_base_msat and fee_proportional_millionths. Forwarding fee = base + amount x proportional / 1,000,000, all in millisatoshi (BOLT 7). #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/07-routing-gossip.md

### 15/ · checked · choix :

**15a (simple)** : Crypto #Layer2 Unlocked #15: Lightning counts in thousandths of a satoshi, smaller than anything Bitcoin itself can record. If a channel closes on-chain, those tiny fractions are rounded away. #Rollups #Lightning

**15b (tech)** : Crypto #Layer2 Unlocked #15: Lightning amounts are in millisatoshi (1 sat = 1,000 msat). Bitcoin outputs can't hold fractions of a sat, so BOLT 3 rounds each commitment output down to whole satoshis. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/03-transactions.md

### 16/ · checked · choix :

**16a (simple)** : Crypto #Layer2 Unlocked #16: Very small Lightning payments in flight can't be enforced on Bitcoin: an output for them would cost more than it's worth. If the channel force-closes, their value goes to miner fees. #Rollups #Lightning

**16b (tech)** : Crypto #Layer2 Unlocked #16: HTLCs below the dust limit are "trimmed" (BOLT 3): no output in the commitment tx, their value goes to fees. Tiny in-flight payments are protected by peer behaviour, not by Bitcoin script. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/03-transactions.md

### 17/ · checked · choix :

**17a (simple)** : Crypto #Layer2 Unlocked #17: A Lightning invoice is a coded payment request: amount, destination, a lock and an expiry. "lnbc" at the start means Bitcoin mainnet. By default it expires after one hour. #Rollups #Lightning

**17b (tech)** : Crypto #Layer2 Unlocked #17: BOLT 11 invoices are bech32 strings: prefix lnbc (mainnet), lntb (testnet), lnbcrt (regtest), then amount, timestamp, tagged fields (payment_hash...) and a signature. Default expiry: 3600 s. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/11-payment-encoding.md

### 18/ · checked · choix :

**18a (simple)** : Crypto #Layer2 Unlocked #18: To receive a Lightning payment, your wallet normally has to be online: it must reveal the secret that completes the payment. If it isn't, the payment just fails and the sender keeps the money. #Rollups #Lightning

**18b (tech)** : Crypto #Layer2 Unlocked #18: Settling an HTLC requires the payee to release the preimage, so the receiving node must be online when the payment arrives. If it isn't, upstream HTLCs time out and the funds go back. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md

### 19/ · checked · choix :

**19a (simple)** : Crypto #Layer2 Unlocked #19: A Lightning channel works like an abacus: coins slide between two sides. You can only receive as much as your partner holds on their side, so a new wallet may need "inbound liquidity". #Rollups #Lightning

**19b (tech)** : Crypto #Layer2 Unlocked #19: Channel capacity is fixed by the funding output; payments only move balance between sides. You send up to your local balance minus reserve and receive up to the remote one: inbound liquidity is a hard limit. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md

### 20/ · checked · choix :

**20a (simple)** : Crypto #Layer2 Unlocked #20: A big Lightning payment can be split into smaller pieces sent along different paths. The recipient waits until all pieces arrive before claiming any of them. #Rollups #Lightning

**20b (tech)** : Crypto #Layer2 Unlocked #20: Multi-path payments send several HTLCs with the same payment_hash and a total_msat field in the onion (BOLT 4). The payee holds them until the total is reached, then releases the preimage. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/04-onion-routing.md

### 21/ · checked · choix :

**21a (simple)** : Crypto #Layer2 Unlocked #21: Lightning started with a safety cap of about 0.168 BTC per channel. Nodes can now opt in to much bigger channels. #Rollups #Lightning

**21b (tech)** : Crypto #Layer2 Unlocked #21: BOLT 2 keeps funding_satoshis below 2^24 sat (16,777,216 sat, about 0.168 BTC) unless the peer supports option_support_large_channel. The spec calls it a temporary, self-imposed limit. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md

### 22/ · checked · choix :

**22a (simple)** : Crypto #Layer2 Unlocked #22: A channel's closing transaction is signed in advance, so its fee may be too low when you need it. Anchor outputs add two tiny 330-sat outputs that let either side boost the fee later. #Rollups #Lightning

**22b (tech)** : Crypto #Layer2 Unlocked #22: With option_anchors, each commitment tx carries two 330-sat anchor outputs, one per peer, so either side can CPFP it at broadcast time instead of guessing a fee rate when signing (BOLT 3). #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/03-transactions.md

### 23/ · checked · choix :

**23a (simple)** : Crypto #Layer2 Unlocked #23: Closing a Lightning channel together is quick: one transaction, coins free right away. Closing alone (force close) works without your partner, but your coins wait out a timelock. #Rollups #Lightning

**23b (tech)** : Crypto #Layer2 Unlocked #23: Mutual close: shutdown, then closing_complete/closing_sig (BOLT 2) build one tx paying both sides, no timelock. Unilateral close broadcasts the latest commitment; the broadcaster waits to_self_delay. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md ; https://github.com/lightning/bolts/blob/master/03-transactions.md

### 24/ · checked · choix :

**24a (simple)** : Crypto #Layer2 Unlocked #24: You can resize a Lightning channel without closing it. "Splicing" adds or removes coins with one on-chain transaction, and the channel keeps working while it confirms. #Rollups #Lightning

**24b (tech)** : Crypto #Layer2 Unlocked #24: Splicing (BOLT 2: splice_init, splice_ack, splice_locked) replaces the funding tx with a new one that adds or removes funds. Normal operation resumes once the splice tx is signed, before it confirms. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/02-peer-protocol.md

### 25/ · checked · choix :

**25a (simple)** : Crypto #Layer2 Unlocked #25: Not every Lightning channel is public. Private channels stay off the shared map, and an invoice can include hints telling the payer how to reach you. #Rollups #Lightning

**25b (tech)** : Crypto #Layer2 Unlocked #25: Unannounced channels never send channel_announcement. To stay reachable, the payee adds r fields (route hints) to its BOLT 11 invoice: extra routing info for a private route to its node. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/07-routing-gossip.md ; https://github.com/lightning/bolts/blob/master/11-payment-encoding.md

### 26/ · checked · choix :

**26a (simple)** : Crypto #Layer2 Unlocked #26: Lightning recipients can hide where they sit in the network. With "blinded paths", the payer only sees the route up to an entry node; the rest is encrypted by the recipient. #Rollups #Lightning

**26b (tech)** : Crypto #Layer2 Unlocked #26: Route blinding (BOLT 4) lets the payee build the last part of the route and encrypt its hops, so the sender learns only the introduction point, not the payee's node or channels. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/04-onion-routing.md

### 27/ · checked · choix :

**27a (simple)** : Crypto #Layer2 Unlocked #27: Lightning's promise: your partner can't take your coins and can't stop you from leaving. The catch: your closing transaction must confirm in time, even when Bitcoin fees spike. #Rollups #Lightning

**27b (tech)** : Crypto #Layer2 Unlocked #27: Lightning security = latest commitment + revocation + timelocks. It holds only if your txs confirm before the relevant timelocks expire, which is why anchors and watchtowers matter in fee spikes. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/03-transactions.md

### 28/ · checked · choix :

**28a (simple)** : Crypto #Layer2 Unlocked #28: Lightning and rollups scale in opposite ways. Lightning keeps payments between channel partners and settles on Bitcoin. Rollups publish their data on Ethereum so anyone can verify. #Rollups #Lightning

**28b (tech)** : Crypto #Layer2 Unlocked #28: Lightning: off-chain state known only to channel peers, enforced on-chain only in disputes. Rollups: all tx data or state diffs posted on L1, state checked by proofs. Different trust and privacy. #Rollups #Lightning

Source : https://github.com/lightning/bolts/blob/master/03-transactions.md ; https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52

### 29/ · checked · choix :

**29a (simple)** : Crypto #Layer2 Unlocked #29: A rollup runs transactions on its own chain, then posts them in batches to Ethereum. Your funds sit in a contract on Ethereum, which is the final judge of who owns what. #Rollups #Lightning

**29b (tech)** : Crypto #Layer2 Unlocked #29: A rollup executes off-chain, posts its tx data to L1, and has an L1 contract accept new state only through a fraud proof game or a validity proof. The L1 bridge contract escrows the assets. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52 ; https://docs.arbitrum.io/how-arbitrum-works/bold/gentle-introduction

### 30/ · checked · choix :

**30a (simple)** : Crypto #Layer2 Unlocked #30: The key rule of a rollup: all the data needed to rebuild its state is published on Ethereum. If the operator disappears, anyone can rebuild the balances from that data. #Rollups #Lightning

**30b (tech)** : Crypto #Layer2 Unlocked #30: Even L2BEAT's Stage 0 requires that open-source software can reconstruct the state from L1-posted data and compare its state roots with the proposed ones. Data on L1 is what makes it a rollup. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52

### 31/ · checked · choix :

**31a (simple)** : Crypto #Layer2 Unlocked #31: If a chain keeps its data off Ethereum, with a committee promising to share it, it's a validium or optimium, not a rollup. If that data is withheld, users may be unable to prove their balances. #Rollups #Lightning

**31b (tech)** : Crypto #Layer2 Unlocked #31: Validiums (validity proofs) and optimiums (fraud proofs) settle on L1 but keep tx data with a Data Availability Committee or an external DA layer. If the data is withheld, exits can freeze. #Rollups #Lightning

Source : https://github.com/ethereum/ethereum-org-website/blob/dev/public/content/developers/docs/scaling/validium/index.md ; https://ethbeat.org/glossary

### 32/ · checked · choix :

**32a (simple)** : Crypto #Layer2 Unlocked #32: A sidechain has its own validators, and its security comes from them, not from Bitcoin or Ethereum. If enough of them collude or get hacked, the main chain can't protect your funds. #Rollups #Lightning

**32b (tech)** : Crypto #Layer2 Unlocked #32: Sidechains run their own consensus and don't post state or tx data back to L1, so they don't inherit its security. Their bridge trusts the sidechain's validators: a malicious quorum of them can commit fraud. #Rollups #Lightning

Source : https://github.com/ethereum/ethereum-org-website/blob/dev/public/content/developers/docs/scaling/sidechains/index.md

### 33/ · checked · choix :

**33a (simple)** : Crypto #Layer2 Unlocked #33: In March 2022, about $625M was stolen from the Ronin bridge, the chain behind Axie Infinity. The attacker controlled 5 of its 9 validator keys, enough to approve withdrawals. #Rollups #Lightning

**33b (tech)** : Crypto #Layer2 Unlocked #33: Ronin bridge, March 2022: about $625M in ETH and USDC drained with withdrawals signed by 5 of 9 validator keys. A rollup bridge would also need a state proof checked on L1, not just a quorum of keys. #Rollups #Lightning

Source : https://halborn.com/explained-the-ronin-hack-march-2022/

### 34/ · checked · choix :

**34a (simple)** : Crypto #Layer2 Unlocked #34: Optimistic rollups post results to Ethereum and assume they're correct. During a waiting window, anyone watching can challenge a wrong result; if it's proven wrong, it's thrown out. #Rollups #Lightning

**34b (tech)** : Crypto #Layer2 Unlocked #34: Optimistic rollups post state claims backed by a bond. If no one wins a challenge within the challenge period, the claim is confirmed; a successful fraud proof rejects it and the bond is lost. #Rollups #Lightning

Source : https://docs.arbitrum.io/how-arbitrum-works/bold/gentle-introduction

### 35/ · checked · choix :

**35a (simple)** : Crypto #Layer2 Unlocked #35: Withdrawing from an OP Stack rollup through its official bridge takes about a week. That delay is the window anyone has to challenge a fake withdrawal. #Rollups #Lightning

**35b (tech)** : Crypto #Layer2 Unlocked #35: OP Stack withdrawal: initiate on L2, prove it on L1 in OptimismPortal against a dispute game, then finalize after a 7-day proof maturity delay. With fault proofs, users may re-prove a withdrawal at any time. #Rollups #Lightning

Source : https://github.com/ethereum-optimism/specs/blob/main/specs/protocol/withdrawals.md ; https://github.com/ethereum-optimism/specs/blob/main/specs/fault-proof/stage-one/optimism-portal.md ("Users may re-prove a withdrawal at any time")

### 36/ · checked · choix :

**36a (simple)** : Crypto #Layer2 Unlocked #36: On Arbitrum One, a new state claim can be challenged for about 6.4 days before it's confirmed. That's why native withdrawals to Ethereum take about a week. #Rollups #Lightning

**36b (tech)** : Crypto #Layer2 Unlocked #36: Arbitrum One's dispute window is 45,818 L1 blocks, about 6.4 days. Proposing an assertion requires a minimum bond of 3,600 ETH, per Arbitrum's chain parameters. #Rollups #Lightning

Source : https://docs.arbitrum.io/for-devs/chain-params

### 37/ · checked · choix :

**37a (simple)** : Crypto #Layer2 Unlocked #37: Don't want to wait a week to leave an optimistic rollup? A liquidity provider can pay you on Ethereum for a fee and take over your pending withdrawal. The provider waits out the week, not you. #Rollups #Lightning

**37b (tech)** : Crypto #Layer2 Unlocked #37: Fast exits are liquidity, not protocol: a liquidity provider takes over your pending withdrawal and pays you on L1 for a fee. The 7-day challenge period still runs; the provider can verify it by running the chain itself. #Rollups #Lightning

Source : https://github.com/ethereum/ethereum-org-website/blob/dev/public/content/developers/docs/scaling/optimistic-rollups/index.md ; https://github.com/ethereum-optimism/specs/blob/main/specs/protocol/withdrawals.md

### 38/ · checked · choix :

**38a (simple)** : Crypto #Layer2 Unlocked #38: When two parties disagree on Arbitrum, Ethereum doesn't re-run everything. They split the disputed work in half again and again until one single step remains. Ethereum checks only that step. #Rollups #Lightning

**38b (tech)** : Crypto #Layer2 Unlocked #38: Arbitrum disputes bisect history commitments: first blocks, then ranges of 2^23 WASM steps, down to one instruction. The L1 OneStepProof contract executes that step and the honest side wins. #Rollups #Lightning

Source : https://docs.arbitrum.io/how-arbitrum-works/bold/gentle-introduction

### 39/ · checked · choix :

**39a (simple)** : Crypto #Layer2 Unlocked #39: Arbitrum runs the core of Geth, Ethereum's most widely used client. For a dispute, that same code is compiled to WebAssembly so a single step can be checked on Ethereum. #Rollups #Lightning

**39b (tech)** : Crypto #Layer2 Unlocked #39: Nitro compiles Geth's core into Arbitrum. Validators run it as native code and switch to the WASM build only when a fraud proof is needed. Arbitrum One moved to Nitro on Aug 31, 2022. #Rollups #Lightning

Source : https://github.com/OffchainLabs/nitro/blob/master/README.md

### 40/ · checked · choix :

**40a (simple)** : Crypto #Layer2 Unlocked #40: Since February 2025, anyone can defend Arbitrum One against a bad state claim, not only a whitelist of validators. One honest validator is enough to win. #Rollups #Lightning

**40b (tech)** : Crypto #Layer2 Unlocked #40: BoLD went live on Arbitrum One and Nova in Feb 2025, replacing allow-listed validators. Bonds attach to assertions, not parties, and disputes are time-bound, so delay attacks can't stall confirmation. #Rollups #Lightning

Source : https://www.theblock.co/post/340278/offchain-labs-releases-arbitrum-bold-on-mainnet-for-permissionless-validation ; https://docs.arbitrum.io/how-arbitrum-works/bold/gentle-introduction

### 41/ · checked · choix :

**41a (simple)** : Crypto #Layer2 Unlocked #41: Since June 2024, anyone can challenge invalid withdrawals on OP Mainnet, with no trusted party needed. An emergency council can still pause the system if the proofs break. #Rollups #Lightning

**41b (tech)** : Crypto #Layer2 Unlocked #41: Permissionless fault proofs (Cannon) went live on OP Mainnet on June 10, 2024. The Security Council can revert to a permissioned mode if the proof system fails: L2BEAT Stage 1, not Stage 2. #Rollups #Lightning

Source : https://www.optimism.io/blog/permissionless-fault-proofs-and-stage-1-arrive-to-the-op-stack

### 42/ · checked · choix :

**42a (simple)** : Crypto #Layer2 Unlocked #42: Optimistic rollups need just one honest watcher. If one party checks the chain and can challenge in time, a false claim can't be confirmed, however many others lie. #Rollups #Lightning

**42b (tech)** : Crypto #Layer2 Unlocked #42: Fraud-proof security is 1-of-N: one honest validator with the data, a bond and L1 access during the window can defeat an invalid assertion. Censoring that party on L1 is the attack to worry about. #Rollups #Lightning

Source : https://docs.arbitrum.io/how-arbitrum-works/bold/gentle-introduction

### 43/ · checked · choix :

**43a (simple)** : Crypto #Layer2 Unlocked #43: ZK rollups attach a mathematical proof to their batches showing the new state is correct. Ethereum checks the proof, so there's no week-long challenge window. #Rollups #Lightning

**43b (tech)** : Crypto #Layer2 Unlocked #43: ZK rollups post a validity proof (SNARK or STARK) with each state update; the L1 verifier accepts the new state root only if the proof checks out. Exits don't wait for a fraud-proof window. #Rollups #Lightning

Source : https://github.com/starknet-io/starknet-docs/blob/main/learn/protocol/sharp.mdx

### 44/ · checked · choix :

**44a (simple)** : Crypto #Layer2 Unlocked #44: With ZK proofs, making the proof takes heavy computing, but checking it is cheap. That gap is what lets Ethereum confirm many transactions at low cost. #Rollups #Lightning

**44b (tech)** : Crypto #Layer2 Unlocked #44: Starknet's SHARP aggregates many Cairo programs into one STARK proof verified on Ethereum, using the "exponential asymmetry" between proving and verifying so apps share the on-chain cost. #Rollups #Lightning

Source : https://github.com/starknet-io/starknet-docs/blob/main/learn/protocol/sharp.mdx

### 45/ · checked · choix :

**45a (simple)** : Crypto #Layer2 Unlocked #45: ZK proofs come in families. STARKs rely only on hash functions and need no secret setup ceremony; the trade-off is bigger proofs than most SNARKs. #Rollups #Lightning

**45b (tech)** : Crypto #Layer2 Unlocked #45: STARKs: transparent (no trusted setup), secured by collision-resistant hashes, seen as post-quantum, larger proofs. SNARKs like Groth16 give tiny proofs (3 group elements) but need a trusted setup. #Rollups #Lightning

Source : https://github.com/ethereum/ethereum-org-website/blob/dev/public/content/zero-knowledge-proofs/index.md ; https://github.com/ethereum/ethereum-org-website/blob/dev/public/content/developers/docs/scaling/zk-rollups/index.md ; https://eprint.iacr.org/2016/260

### 46/ · checked · choix :

**46a (simple)** : Crypto #Layer2 Unlocked #46: Not all "zkEVMs" are equal. Vitalik Buterin sorted them into types 1 to 4: type 1 matches Ethereum exactly but is slow to prove; type 4 is fast to prove but less compatible. #Rollups #Lightning

**46b (tech)** : Crypto #Layer2 Unlocked #46: Vitalik's Aug 2022 zkEVM taxonomy: Type 1 fully Ethereum-equivalent, Type 2 fully EVM-equivalent, Type 2.5 same except gas costs, Type 3 almost EVM-equivalent, Type 4 high-level-language equivalent. #Rollups #Lightning

Source : https://vitalik.eth.limo/general/2022/08/04/zkevm.html

### 47/ · checked · choix :

**47a (simple)** : Crypto #Layer2 Unlocked #47: Most rollups have a single "sequencer" that orders transactions. It can't steal your funds, but it can delay or ignore your transaction for a while. #Rollups #Lightning

**47b (tech)** : Crypto #Layer2 Unlocked #47: Most rollups run one sequencer that orders txs and posts batches. Proofs stop it from forging state, but it can reorder, delay or censor, which is why forced inclusion through L1 exists. #Rollups #Lightning

Source : https://docs.arbitrum.io/sequencer

### 48/ · checked · choix :

**48a (simple)** : Crypto #Layer2 Unlocked #48: The instant "confirmed" you see on a rollup is the sequencer's promise. The real guarantee comes later, once the batch is posted on Ethereum and finalized there. #Rollups #Lightning

**48b (tech)** : Crypto #Layer2 Unlocked #48: Arbitrum's sequencer feed gives "soft finality" in under a second, which relies on the sequencer behaving honestly. The stronger guarantee comes once the batch lands on L1 and that block finalizes. #Rollups #Lightning

Source : https://docs.arbitrum.io/sequencer

### 49/ · checked · choix :

**49a (simple)** : Crypto #Layer2 Unlocked #49: If Arbitrum's sequencer ignores you, you can send your transaction through Ethereum instead. After at most 24 hours, it can be forced into the chain without the sequencer. #Rollups #Lightning

**49b (tech)** : Crypto #Layer2 Unlocked #49: Arbitrum One: send a message to the L1 delayed inbox. If the sequencer hasn't included it after the force-include period (7,200 L1 blocks, 24 h), anyone can force it in. #Rollups #Lightning

Source : https://docs.arbitrum.io/for-devs/chain-params ; https://docs.arbitrum.io/sequencer

### 50/ · checked · choix :

**50a (simple)** : Crypto #Layer2 Unlocked #50: On OP Stack chains you can send a transaction straight to a contract on Ethereum, and the chain must include it. If the sequencer stalls, it gets in within at most about 12 hours. #Rollups #Lightning

**50b (tech)** : Crypto #Layer2 Unlocked #50: OP Stack forced txs are deposits sent to OptimismPortal on L1, which must be included within the 12-hour sequencing window. Past that, nodes derive blocks with only the forced deposits. #Rollups #Lightning

Source : https://docs.optimism.io/stack/transactions/forced-transaction

### 51/ · checked · choix :

**51a (simple)** : Crypto #Layer2 Unlocked #51: Some rollups drop the central sequencer and let Ethereum's own block proposers order their transactions. They're called "based" rollups; Taiko calls itself the first one. #Rollups #Lightning

**51b (tech)** : Crypto #Layer2 Unlocked #51: Based rollups are sequenced by L1: Ethereum block proposers include rollup batches, so ordering and censorship resistance come from L1, not from a single operator. Taiko Alethia is one. #Rollups #Lightning

Source : https://github.com/taikoxyz/taiko-mono/blob/main/README.md

### 52/ · checked · choix :

**52a (simple)** : Crypto #Layer2 Unlocked #52: Rollup contracts on Ethereum can often be upgraded. Whoever holds the upgrade keys could change the rules, so "who can upgrade, and how fast?" matters as much as the proofs. #Rollups #Lightning

**52b (tech)** : Crypto #Layer2 Unlocked #52: Most rollup bridges sit behind upgradeable contracts. An instant upgrade by an admin multisig can swap the verifier or bridge logic and bypass proofs entirely. Exit windows exist to counter that. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52

### 53/ · checked · choix :

**53a (simple)** : Crypto #Layer2 Unlocked #53: L2BEAT (now ETHBEAT) ranks rollups in "stages": Stage 0 still leans heavily on its operators, Stage 1 has limited "training wheels", Stage 2 has none. It measures how much power admins keep. #Rollups #Lightning

**53b (tech)** : Crypto #Layer2 Unlocked #53: L2BEAT Stages: 0 = full training wheels, 1 = limited training wheels, 2 = no training wheels. The framework was inspired by Vitalik's proposed milestones for rollups taking off training wheels. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52 ; https://docs.arbitrum.io/how-arbitrum-works/bold/gentle-introduction ; https://ethbeat.org/l2beat-is-now-ethbeat

### 54/ · checked · choix :

**54a (simple)** : Crypto #Layer2 Unlocked #54: To reach L2BEAT Stage 1, a rollup's emergency council needs at least 8 public members, at least half from outside the team, and a 75% vote to act. #Rollups #Lightning

**54b (tech)** : Crypto #Layer2 Unlocked #54: L2BEAT Stage 1: Security Council with 8+ members, 50%+ outside the core org, 75% threshold. Other upgrades must leave users at least 7 days to exit, and the proof system must be fully functional. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52

### 55/ · checked · choix :

**55a (simple)** : Crypto #Layer2 Unlocked #55: L2BEAT Stage 2 means "no training wheels": anyone can submit fraud proofs, and the emergency council can only act on bugs that can be proven on-chain. #Rollups #Lightning

**55b (tech)** : Crypto #Layer2 Unlocked #55: L2BEAT Stage 2: permissionless proof system, Security Council limited to soundness errors adjudicable on-chain, and at least 30 days for users to exit before any unwanted upgrade. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52 ; https://ethbeat.org/stages

### 56/ · checked · choix :

**56a (simple)** : Crypto #Layer2 Unlocked #56: An "exit window" is the time you get to withdraw after an upgrade is announced, before it takes effect. If you dislike the change, you leave first. No window means you simply have to trust the admins. #Rollups #Lightning

**56b (tech)** : Crypto #Layer2 Unlocked #56: Exit window = upgrade delay minus withdrawal time. In L2BEAT's worked example, a 13-day upgrade delay on a rollup where withdrawals take about a week leaves about 6 days to exit. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52

### 57/ · checked · choix :

**57a (simple)** : Crypto #Layer2 Unlocked #57: Since Ethereum's Dencun upgrade on March 13, 2024, rollups can post their data in "blobs": cheap, temporary data attachments made for them. #Rollups #Lightning

**57b (tech)** : Crypto #Layer2 Unlocked #57: EIP-4844 (Dencun, March 13, 2024) added type-3 blob transactions. A blob is 4096 field elements x 32 bytes = 128 KiB. At launch: target 3, max 6 blobs per block. #Rollups #Lightning

Source : https://eips.ethereum.org/EIPS/eip-4844 ; https://eips.ethereum.org/EIPS/eip-7569

### 58/ · checked · choix :

**58a (simple)** : Crypto #Layer2 Unlocked #58: Blob data doesn't stay on Ethereum forever: nodes keep it for about 18 days. That's long enough for anyone to download it and check, or challenge, a rollup. #Rollups #Lightning

**58b (tech)** : Crypto #Layer2 Unlocked #58: Consensus nodes serve blob sidecars for MIN_EPOCHS_FOR_BLOB_SIDECARS_REQUESTS = 4096 epochs, about 18 days. What stays on-chain is the KZG commitment, referenced by a versioned hash. #Rollups #Lightning

Source : https://eips.ethereum.org/EIPS/eip-4844

### 59/ · checked · choix :

**59a (simple)** : Crypto #Layer2 Unlocked #59: Smart contracts can't read blob contents. They only see a short fingerprint of each blob, plus a tool to check that a given piece of data really belongs to it. #Rollups #Lightning

**59b (tech)** : Crypto #Layer2 Unlocked #59: Blob data isn't accessible to the EVM. Contracts get its versioned hash via the BLOBHASH opcode and can verify KZG proofs of single points with the point evaluation precompile at address 0x0A. #Rollups #Lightning

Source : https://eips.ethereum.org/EIPS/eip-4844

### 60/ · checked · choix :

**60a (simple)** : Crypto #Layer2 Unlocked #60: Blobs have their own fee market, separate from normal Ethereum gas. Rollup data no longer competes head-on with swaps and NFT mints for block space. #Rollups #Lightning

**60b (tech)** : Crypto #Layer2 Unlocked #60: EIP-4844 adds blob gas with its own EIP-1559-style base fee, driven by blob usage versus target. Each blob uses GAS_PER_BLOB = 131,072 blob gas, priced separately from execution gas. #Rollups #Lightning

Source : https://eips.ethereum.org/EIPS/eip-4844

### 61/ · checked · choix :

**61a (simple)** : Crypto #Layer2 Unlocked #61: Right after Dencun, fees on several L2s fell sharply. On Base, the median fee went from about $0.50 to about $0.0012, The Block reported. #Rollups #Lightning

**61b (tech)** : Crypto #Layer2 Unlocked #61: Post-Dencun (March 2024), The Block reported Base median fees falling from about $0.5 to about $0.0012, and Starknet swap fees from over $6.8 to about $0.04, as rollups moved data to blobs. #Rollups #Lightning

Source : https://www.theblock.co/news/ecosystems/2024-03-14-ethereum-layer-2s-show-dramatic-drop-in-transaction-fees-after-dencun-282417

### 62/ · checked · choix :

**62a (simple)** : Crypto #Layer2 Unlocked #62: Ethereum's Pectra upgrade (May 7, 2025) raised blob capacity to a target of 6 and a max of 9 blobs per block, up from 3 and 6. More room for rollup data. #Rollups #Lightning

**62b (tech)** : Crypto #Layer2 Unlocked #62: EIP-7691, shipped in Pectra on May 7, 2025, moved blob target/max per block from 3/6 to 6/9, doubling the target data capacity available to rollups. #Rollups #Lightning

Source : https://eips.ethereum.org/EIPS/eip-7691 ; https://eips.ethereum.org/EIPS/eip-7600

### 63/ · checked · choix :

**63a (simple)** : Crypto #Layer2 Unlocked #63: Since Fusaka (Dec 3, 2025), Ethereum nodes no longer have to download every blob in full. They check samples, so the network can carry more blobs without more bandwidth per node. #Rollups #Lightning

**63b (tech)** : Crypto #Layer2 Unlocked #63: Fusaka (Dec 3, 2025) activated PeerDAS (EIP-7594): nodes verify blob availability by sampling parts of blobs instead of downloading them whole, so blob limits can rise without matching bandwidth. #Rollups #Lightning

Source : https://blog.ethereum.org/2025/11/06/fusaka-mainnet-announcement

### 64/ · checked · choix :

**64a (simple)** : Crypto #Layer2 Unlocked #64: After Fusaka, Ethereum raised blob limits through small "blob-only" upgrades: target 10 and max 15 in December 2025, then 14 and 21 in January 2026. #Rollups #Lightning

**64b (tech)** : Crypto #Layer2 Unlocked #64: Blob Parameter Only (BPO) forks change just blob settings: BPO1 (Dec 9, 2025) set target/max to 10/15, BPO2 (Jan 7, 2026) to 14/21. No other protocol change rides along. #Rollups #Lightning

Source : https://blog.ethereum.org/2025/11/06/fusaka-mainnet-announcement ; https://eips.ethereum.org/EIPS/eip-8135

### 65/ · checked · choix :

**65a (simple)** : Crypto #Layer2 Unlocked #65: Your L2 fee mainly pays for two things: running your transaction on the L2, and publishing its data on Ethereum. On chains using blobs, the Ethereum part depends mostly on the blob fee. #Rollups #Lightning

**65b (tech)** : Crypto #Layer2 Unlocked #65: OP Stack fee = execution gas x (base + priority fee) + an L1 data fee for posting the tx's compressed data to Ethereum, plus an operator fee since Isthmus. Blob base fee largely sets the L1 part. #Rollups #Lightning

Source : https://docs.optimism.io/stack/transactions/fees

### 66/ · checked · choix :

**66a (simple)** : Crypto #Layer2 Unlocked #66: Rollups compress transactions before posting them to Ethereum, because every byte published costs money. Smaller data, smaller fees. #Rollups #Lightning

**66b (tech)** : Crypto #Layer2 Unlocked #66: Arbitrum Nitro's ArbOS includes a batching and compression system built to minimize L1 costs: transactions reach Ethereum as compressed batches, not one by one. #Rollups #Lightning

Source : https://github.com/OffchainLabs/nitro/blob/master/README.md

### 67/ · checked · choix :

**67a (simple)** : Crypto #Layer2 Unlocked #67: L2s in one line: Lightning = coins locked with your partner, safe if you watch the chain. Rollups = data on Ethereum, safe if proofs and upgrade rules hold. Sidechains = trust their validators. Follow for more. #Rollups #Lightning

**67b (tech)** : Crypto #Layer2 Unlocked #67: Recap: Lightning relies on timelocks and penalties, optimistic rollups on 1 honest challenger, ZK rollups on validity proofs, all on L1 data and safe upgrade keys. Sidechains rely on validators. Follow for more. #Rollups #Lightning

Source : https://medium.com/l2beat/stages-update-security-council-requirements-4c79cea8ef52 ; https://github.com/lightning/bolts/blob/master/03-transactions.md ; https://docs.arbitrum.io/how-arbitrum-works/bold/gentle-introduction
