# Thread : The dark forest: MEV explained

- Sujet : 7. MEV et mempool : la forêt sombre
- Angle : ce qui arrive à ta transaction entre "Envoyer" et l'inclusion dans un bloc.
- Préfixe : `Crypto #MEV Dark Forest #N:`
- Hashtags fixes : #DeFi #CryptoTrading
- Statut : draft (posts "checked" vérifiés contre la source indiquée ; posts "draft" à revérifier avant publication)

Notes avant publication :
- Post 43 (ePBS) : revérifié le 10/11/2026 : Glamsterdam pas encore sur le mainnet (fork Hoodi du 26/10 reporté), ePBS (EIP-7732) reste le headliner CL ; FOCIL (post 44) a été déplacé vers Hegota. Vérifier le statut le jour de la publication.
- Post 36 (procès Peraire-Bueno) : relu le 10/11/2026. Dernière source lue (Inner City Press, 30/03/2026) : le parquet veut rejuger au plus vite, la défense demande une date à partir de juin 2027, aucune date fixée par le juge, motion d'acquittement (Rule 29) en attente. Rien de plus récent trouvé (CourtListener inaccessible). Vérifier le statut le jour de la publication.
- Posts 14, 15, 38, 48 : relus le 10/11/2026 contre les sources indiquées et passés en "checked" (textes ajustés à ce que disent les sources).
- Posts 18-20 : article Paradigm relu le 10/11/2026 ; 18 reformulé (l'article ne parle pas de "higher fee") ; Liu Cixin sourcé via Wikipedia.

---

### 1/ · checked · choix :

**1a (simple)** : Crypto #MEV Dark Forest #1: You click "Send". Before your transaction lands in a block, it waits in a public room where bots read it and look for a way to profit from it. That profit is called MEV. Welcome to the dark forest. #DeFi #CryptoTrading

**1b (tech)** : Crypto #MEV Dark Forest #1: Between signing and inclusion, an Ethereum tx usually sits in the public mempool. Searcher bots simulate it and bid to place their own txs around it. The value they capture is MEV, and it shapes how blocks get built. #DeFi #CryptoTrading

**1c (variante)** : Crypto #MEV Dark Forest #1: Your DeFi trade is public before it is final. Bots see it, copy it, or trade around it, all in the few seconds before the next block. This series explains how, and how to protect yourself. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 2/ · checked · choix :

**2a (simple)** : Crypto #MEV Dark Forest #2: MEV is the extra money a block producer (or a bot paying them) can make by choosing which transactions go in a block, which stay out, and in what order. #DeFi #CryptoTrading

**2b (tech)** : Crypto #MEV Dark Forest #2: MEV = the maximum value extractable from block production beyond the standard block reward and gas fees, by including, excluding or reordering transactions in a block. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 3/ · checked · choix :

**3a (simple)** : Crypto #MEV Dark Forest #3: The term MEV comes from a 2019 research paper called "Flash Boys 2.0", named after Michael Lewis's book about high-speed traders on Wall Street. #DeFi #CryptoTrading

**3b (tech)** : Crypto #MEV Dark Forest #3: "Miner extractable value" was coined in "Flash Boys 2.0" by Daian, Goldfeder, Kell, Li, Zhao, Bentov, Breidenbach and Juels (arXiv 1904.05234, April 2019), a study of frontrunning bots on DEXes. #DeFi #CryptoTrading

Source : https://arxiv.org/abs/1904.05234

### 4/ · checked · choix :

**4a (simple)** : Crypto #MEV Dark Forest #4: The "M" in MEV used to mean "miner". Ethereum has no miners since it moved to proof-of-stake, so it now means "maximal". The idea stayed the same. #DeFi #CryptoTrading

**4b (tech)** : Crypto #MEV Dark Forest #4: MEV was first "miner extractable value". With proof-of-stake, validators order blocks, so the community renamed it "maximal extractable value". The mechanics apply to any block producer. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 5/ · checked · choix :

**5a (simple)** : Crypto #MEV Dark Forest #5: The mempool is the waiting room of a blockchain: transactions sent but not yet in a block. On Ethereum it is public, so anyone running a node can read your pending trade. #DeFi #CryptoTrading

**5b (tech)** : Crypto #MEV Dark Forest #5: Ethereum nodes gossip pending txs to peers and keep them in a local mempool until inclusion. Anyone can subscribe to that stream, see calldata, amounts and fees, and simulate the result before it is final. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 6/ · checked · choix :

**6a (simple)** : Crypto #MEV Dark Forest #6: On Ethereum, a new block slot comes every 12 seconds. That short window is when bots spot your transaction and race to act on it. #DeFi #CryptoTrading

**6b (tech)** : Crypto #MEV Dark Forest #6: Ethereum proof-of-stake runs on 12-second slots, each with one chosen proposer. MEV competition is a race inside that slot: find an opportunity, simulate, bid, and get into the block before the slot ends. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/blocks/

### 7/ · checked · choix :

**7a (simple)** : Crypto #MEV Dark Forest #7: The hunters in the dark forest are called "searchers": bots that scan the chain all day for profitable moves and pay block producers to include their transactions. #DeFi #CryptoTrading

**7b (tech)** : Crypto #MEV Dark Forest #7: Searchers run algorithms over mempool and chain state to find MEV, then pay high fees or direct bids to get their txs placed. Competition pushes most of the profit to the block producer. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 8/ · checked · choix :

**8a (simple)** : Crypto #MEV Dark Forest #8: Front-running: a bot sees your pending trade, sends the same kind of trade first with a higher fee, and gets the better price before you. #DeFi #CryptoTrading

**8b (tech)** : Crypto #MEV Dark Forest #8: Front-running = placing a tx right before a known pending tx in the same block, usually by outbidding its priority fee, to profit from the price move or state change it will cause. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 9/ · checked · choix :

**9a (simple)** : Crypto #MEV Dark Forest #9: A sandwich attack: a bot buys just before you, your buy pushes the price up, the bot sells right after you. You pay more, the bot keeps the difference. #DeFi #CryptoTrading

**9b (tech)** : Crypto #MEV Dark Forest #9: Sandwich = front-run + victim + back-run in one block. The searcher buys the token, the victim's swap executes at a worse price on the AMM curve, and the searcher sells into the higher price. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 10/ · checked · choix :

**10a (simple)** : Crypto #MEV Dark Forest #10: Your "slippage tolerance" is the worst price you accept. For a sandwich bot, it is also the budget: it can push the price up to that limit and your trade still goes through. #DeFi #CryptoTrading

**10b (tech)** : Crypto #MEV Dark Forest #10: A swap's minimum-output (amountOutMin) caps a sandwich. The attacker sizes its front-run so the victim still clears that floor. A wide slippage setting means a bigger extractable gap. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 11/ · checked · choix :

**11a (simple)** : Crypto #MEV Dark Forest #11: Back-running is the gentle cousin: a bot places its trade right after yours, for example to fix a price your trade moved. You don't lose anything from it. #DeFi #CryptoTrading

**11b (tech)** : Crypto #MEV Dark Forest #11: Back-running = inserting a tx immediately after a target tx (a large swap, an oracle update) to capture the state it creates. Unlike front-running, it does not worsen the target's execution. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 12/ · checked · choix :

**12a (simple)** : Crypto #MEV Dark Forest #12: Not all MEV is bad. When a token is cheaper on one exchange than another, bots buy low and sell high at once. That keeps prices aligned across DEXes. #DeFi #CryptoTrading

**12b (tech)** : Crypto #MEV Dark Forest #12: DEX arbitrage is the most common MEV: buy on the pool with the lower price, sell on the other, atomically in one tx. If any leg fails, the whole tx reverts. It keeps AMM prices in line. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 13/ · checked · choix :

**13a (simple)** : Crypto #MEV Dark Forest #13: Lending apps need someone to close loans that are no longer safe. Bots race to do it because the protocol pays them a bonus. That race is MEV, and it keeps lenders solvent. #DeFi #CryptoTrading

**13b (tech)** : Crypto #MEV Dark Forest #13: Liquidations: when a borrower's collateral falls below the required ratio, anyone can repay debt and seize collateral plus a fee. Searchers watch positions and race to liquidate first. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/

### 14/ · checked · choix :

**14a (simple)** : Crypto #MEV Dark Forest #14: A bot doesn't need to be rich to do arbitrage. With a flash loan it borrows with no collateral, trades, and repays plus a small fee, all in one transaction. If it can't repay, the whole transaction is cancelled. #DeFi #CryptoTrading

**14b (tech)** : Crypto #MEV Dark Forest #14: Aave flash loans: borrow with no collateral, return amount + fee (0.05% at V3 deployment, changeable by governance) in the same tx, or it reverts. Aave cites arbitrage and liquidations without holding the principal. #DeFi #CryptoTrading

Source : https://aave.com/docs/aave-v3/guides/flash-loans (+ https://ethereum.org/en/developers/docs/mev/)

### 15/ · checked · choix :

**15a (simple)** : Crypto #MEV Dark Forest #15: Just-in-time liquidity: a bot sees a very large trade coming, adds liquidity to the pool just before it, collects a share of the trading fee, and pulls out right after, all in the same block. #DeFi #CryptoTrading

**15b (tech)** : Crypto #MEV Dark Forest #15: JIT liquidity on Uniswap v3: an LP mints a concentrated position in the tick a large pending swap crosses, then burns it right after, in one block, often via a Flashbots bundle. >95% came from one address (2021-22). #DeFi #CryptoTrading

Source : https://blog.uniswap.org/jit-liquidity (Uniswap Labs, Sept 2022 ; l'ancien lien Kaiko redirige vers app.kaiko.com)

### 16/ · checked · choix :

**16a (simple)** : Crypto #MEV Dark Forest #16: Before private auctions, bots fought in public: each raised its fee to jump ahead of the others, again and again. Researchers called these fee wars "priority gas auctions". #DeFi #CryptoTrading

**16b (tech)** : Crypto #MEV Dark Forest #16: Flash Boys 2.0 documented priority gas auctions (PGAs): bots rebroadcast the same tx with ever higher gas prices, outbidding each other in real time for the earliest position in the block. #DeFi #CryptoTrading

Source : https://arxiv.org/abs/1904.05234

### 17/ · checked · choix :

**17a (simple)** : Crypto #MEV Dark Forest #17: Those fee wars hurt everyone: they pushed gas prices up for normal users and filled blocks with failed bot transactions that still paid fees. #DeFi #CryptoTrading

**17b (tech)** : Crypto #MEV Dark Forest #17: Public PGAs had side effects: network congestion, higher gas for all users, and blockspace wasted on reverted txs from losing bots. Flashbots Auction was built to move this bidding off-chain. #DeFi #CryptoTrading

Source : https://docs.flashbots.net/flashbots-auction/overview

### 18/ · checked · choix :

**18a (simple)** : Crypto #MEV Dark Forest #18: Some bots don't even need to understand your transaction. They look for any pending transaction that would make money, copy it with their own address swapped in, and try to get the copy in first. #DeFi #CryptoTrading

**18b (tech)** : Crypto #MEV Dark Forest #18: A generalized frontrunner looks for any pending tx it can profitably copy, replacing the addresses with its own. It can also execute the tx and copy profitable internal calls from its trace. No need to understand the contract. #DeFi #CryptoTrading

Source : https://www.paradigm.xyz/writing/ethereum-is-a-dark-forest

### 19/ · checked · choix :

**19a (simple)** : Crypto #MEV Dark Forest #19: In Aug 2020, two researchers tried to rescue about $12,000 of tokens anyone could claim. A bot saw their transaction and claimed the tokens first. They titled their story "Ethereum is a Dark Forest". #DeFi #CryptoTrading

**19b (tech)** : Crypto #MEV Dark Forest #19: "Ethereum is a Dark Forest" (Robinson & Konstantopoulos, Aug 28 2020): ≈$12k of Uniswap v2 LP tokens sat in a pair contract; anyone could call burn(). A generalized frontrunner copied their call and took them. #DeFi #CryptoTrading

Source : https://www.paradigm.xyz/writing/ethereum-is-a-dark-forest

### 20/ · checked · choix :

**20a (simple)** : Crypto #MEV Dark Forest #20: Why "dark forest"? It comes from Liu Cixin's sci-fi novel The Dark Forest: a universe where anything that reveals itself gets destroyed by hidden predators. Like a pending transaction. #DeFi #CryptoTrading

**20b (tech)** : Crypto #MEV Dark Forest #20: The metaphor is from Liu Cixin's The Dark Forest: being detected means being destroyed. In the mempool, a profitable pending tx is detected by simulation, then copied or front-run by bots. #DeFi #CryptoTrading

Source : https://www.paradigm.xyz/writing/ethereum-is-a-dark-forest + https://en.wikipedia.org/wiki/The_Dark_Forest (auteur Liu Cixin, non nommé dans l'article Paradigm)

### 21/ · checked · choix :

**21a (simple)** : Crypto #MEV Dark Forest #21: In Sept 2020, a white hat found ≈25,000 ETH at risk in a DeFi contract. To rescue it without bots stealing it, his team sent the transactions privately to a mining pool instead of the public mempool. #DeFi #CryptoTrading

**21b (tech)** : Crypto #MEV Dark Forest #21: "Escaping the Dark Forest" (samczsun, Sept 2020): a Lien Finance contract held ≈25k ETH (over $9.6M). The rescue txs went to SparkPool via a private submission feature and never touched the public mempool. #DeFi #CryptoTrading

Source : https://samczsun.com/escaping-the-dark-forest/

### 22/ · checked · choix :

**22a (simple)** : Crypto #MEV Dark Forest #22: On Black Thursday (March 12, 2020), Ethereum was so congested that some MakerDAO collateral auctions got no competing bids. About $8.3M of ETH went to bidders who paid 0 DAI. #DeFi #CryptoTrading

**22b (tech)** : Crypto #MEV Dark Forest #22: MakerDAO, March 12 2020: gas spiked, most keeper bots failed to bid, and some liquidation auctions closed with a single 0 DAI bid. ≈$8.32M of ETH collateral was won for nothing. #DeFi #CryptoTrading

Source : https://medium.com/@whiterabbit_hq/black-thursday-for-makerdao-8-32-million-was-liquidated-for-0-dai-36b83cac56b6

### 23/ · checked · choix :

**23a (simple)** : Crypto #MEV Dark Forest #23: MEV can even threaten the chain itself. If an old block held enough profit, a block producer could be tempted to rewrite history to grab it. Researchers call this a "time-bandit" attack. #DeFi #CryptoTrading

**23b (tech)** : Crypto #MEV Dark Forest #23: Time-bandit attacks (Flash Boys 2.0): if the MEV in past blocks exceeds the cost of reorganizing the chain, a producer is incentivized to fork and re-mine those blocks to capture it. A consensus risk. #DeFi #CryptoTrading

Source : https://arxiv.org/abs/1904.05234

### 24/ · checked · choix :

**24a (simple)** : Crypto #MEV Dark Forest #24: Flashbots, a research group, gave bots a private channel: they send a sealed bid with a package of transactions straight to block producers, instead of fighting in public. #DeFi #CryptoTrading

**24b (tech)** : Crypto #MEV Dark Forest #24: Flashbots bundles: an ordered list of txs, executed all-or-nothing, sent privately with a sealed bid. A bundle that would revert is simply not included, so losing searchers pay nothing. #DeFi #CryptoTrading

Source : https://docs.flashbots.net/flashbots-auction/overview

### 25/ · checked · choix :

**25a (simple)** : Crypto #MEV Dark Forest #25: Since the London upgrade (Aug 2021), Ethereum burns the base part of every fee. Only the tip goes to the block producer, and bots compete with tips or direct payments. #DeFi #CryptoTrading

**25b (tech)** : Crypto #MEV Dark Forest #25: EIP-1559 (London, Aug 2021): base fee is burned, the priority fee goes to the block's fee recipient. MEV is paid on top, via priority fees or a direct transfer, and none of it is burned. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/gas/

### 26/ · checked · choix :

**26a (simple)** : Crypto #MEV Dark Forest #26: Today on Ethereum, the validator who proposes a block usually doesn't build it. Specialists called builders assemble the block, and the validator picks the one that pays it most. #DeFi #CryptoTrading

**26b (tech)** : Crypto #MEV Dark Forest #26: Proposer-builder separation (PBS): builders compete to assemble the most valuable block from public and private order flow; the proposer just selects the highest bid. Validators need no MEV skills. #DeFi #CryptoTrading

Source : https://ethereum.org/en/roadmap/pbs/

### 27/ · checked · choix :

**27a (simple)** : Crypto #MEV Dark Forest #27: MEV-Boost, made by Flashbots, went live with the Merge on Sept 15, 2022. Validators adopted it fast: about 90% of them ran it within two months. #DeFi #CryptoTrading

**27b (tech)** : Crypto #MEV Dark Forest #27: MEV-Boost (out-of-protocol PBS sidecar) activated at the Merge, block 15537940, Sept 15 2022. Flashbots reports 50% of validators ran it after one month and ≈90% after two. #DeFi #CryptoTrading

Source : https://collective.flashbots.net/t/merge-anniversary-a-year-in-review/2400

### 28/ · checked · choix :

**28a (simple)** : Crypto #MEV Dark Forest #28: Between builders and validators sit "relays": middlemen trusted by both sides. They check the block and its payment, and hide its contents until the validator commits to it. #DeFi #CryptoTrading

**28b (tech)** : Crypto #MEV Dark Forest #28: In MEV-Boost, relays escrow builder blocks. The proposer signs a blinded header for the best bid; only then does the relay release the payload. Builders trust relays not to steal MEV, proposers trust bids are real. #DeFi #CryptoTrading

Source : https://docs.flashbots.net/flashbots-mev-boost/introduction

### 29/ · checked · choix :

**29a (simple)** : Crypto #MEV Dark Forest #29: One year after the Merge, a handful of relays carried most Ethereum blocks: the top 6 handled about 90% of them, and no single relay had more than 25%. #DeFi #CryptoTrading

**29b (tech)** : Crypto #MEV Dark Forest #29: Flashbots' Merge anniversary review (Sept 2023): top 6 MEV-Boost relays held ≈90% of slot share, none above 25%. The top 4 builders also held ≈90% of builder market share. #DeFi #CryptoTrading

Source : https://collective.flashbots.net/t/merge-anniversary-a-year-in-review/2400

### 30/ · checked · choix :

**30a (simple)** : Crypto #MEV Dark Forest #30: Thousands of validators, but very few block builders. In early October 2024, just two builders produced about 89% of all Ethereum blocks. A new kind of centralization. #DeFi #CryptoTrading

**30b (tech)** : Crypto #MEV Dark Forest #30: Builder concentration: per EF researcher Toni Wahrstätter, beaverbuild and Titan built 88.7% of Ethereum mainnet blocks in the first two weeks of Oct 2024. Exclusive order flow drives the edge. #DeFi #CryptoTrading

Source : https://cointelegraph.com/news/two-ethereum-entities-produced-88-blocks-centralization-concerns

### 31/ · checked · choix :

**31a (simple)** : Crypto #MEV Dark Forest #31: MEV peaks during market chaos. In Ethereum's first post-Merge year, the biggest MEV period was the FTX collapse, followed by the USDC depeg. Panic means price gaps, and price gaps mean bots. #DeFi #CryptoTrading

**31b (tech)** : Crypto #MEV Dark Forest #31: Largest MEV episodes in the first post-Merge year (Flashbots): FTX collapse 14,585 ETH, SVB/USDC depeg 7,694 ETH, Vyper exploit 7,187 ETH paid to validators. Volatility creates arbitrage. #DeFi #CryptoTrading

Source : https://collective.flashbots.net/t/merge-anniversary-a-year-in-review/2400

### 32/ · checked · choix :

**32a (simple)** : Crypto #MEV Dark Forest #32: Some blocks are jackpots. In Ethereum's first year after the Merge, the largest MEV payment for a single block was 678 ETH, paid to one validator. #DeFi #CryptoTrading

**32b (tech)** : Crypto #MEV Dark Forest #32: MEV rewards are very uneven: the record single MEV-Boost payout in the first post-Merge year was 678 ETH, while total proposer income over that year was over 300,000 ETH (fees + MEV). #DeFi #CryptoTrading

Source : https://collective.flashbots.net/t/merge-anniversary-a-year-in-review/2400

### 33/ · checked · choix :

**33a (simple)** : Crypto #MEV Dark Forest #33: After the US sanctioned Tornado Cash in Aug 2022, some relays refused blocks with its transactions. At the peak, on Nov 21, 2022, about 79% of Ethereum blocks went through such relays. #DeFi #CryptoTrading

**33b (tech)** : Crypto #MEV Dark Forest #33: OFAC censorship via PBS: relays filtering sanctioned addresses peaked at 79% of blocks on Nov 21 2022, fell to 17% in April 2023 as non-censoring relays and builders gained share. #DeFi #CryptoTrading

Source : https://collective.flashbots.net/t/merge-anniversary-a-year-in-review/2400

### 34/ · checked · choix :

**34a (simple)** : Crypto #MEV Dark Forest #34: The most famous sandwich bot has a name: jaredfromsubway.eth. During the April 2023 memecoin frenzy it spent millions of dollars on fees to sandwich traders. #DeFi #CryptoTrading

**34b (tech)** : Crypto #MEV Dark Forest #34: jaredfromsubway.eth, a sandwich bot named for the sandwich chain, became one of Ethereum's top gas spenders in April 2023, paying millions in fees to front-run memecoin buyers like PEPE traders. #DeFi #CryptoTrading

Source : https://www.theblock.co/post/227110

### 35/ · checked · choix :

**35a (simple)** : Crypto #MEV Dark Forest #35: Predators can become prey. In April 2023, a validator used a flaw in MEV-Boost software to see sandwich bots' trades before committing, and turned them against the bots: about $25M taken in one block. #DeFi #CryptoTrading

**35b (tech)** : Crypto #MEV Dark Forest #35: April 2023: a malicious validator exploited a MEV-Boost relay flaw to see bundle contents before the block was final, then reordered txs to drain sandwich bots via bait trades. ≈$25M in ≈12 seconds. #DeFi #CryptoTrading

Source : https://www.theblock.co/post/378101/massive-overstep-mistrial-declared-for-mev-brothers-accused-of-25-million-fraud-on-ethereum

### 36/ · checked · choix :

**36a (simple)** : Crypto #MEV Dark Forest #36: US prosecutors charged two MIT-educated brothers over that $25M MEV exploit. Their trial in New York ended in a mistrial in November 2025: the jury could not agree. Prosecutors want to retry them. #DeFi #CryptoTrading

**36b (tech)** : Crypto #MEV Dark Forest #36: US v. Peraire-Bueno (SDNY): the brothers accused of the April 2023 $25M MEV-Boost exploit went to trial in 2025. After four weeks the jury deadlocked; mistrial in Nov 2025. The government seeks a retrial. #DeFi #CryptoTrading

Source : https://cointelegraph.com/news/mev-bot-trial-ends-in-mistrial-after-jury-deadlock-on-brothers-verdict + https://innercitypress.com/cryptobrostrial26perairebuenoicp033026.html (30/03/2026 : le parquet se dit prêt à rejuger « as soon as practicable », la défense demande juin 2027 ; aucune date fixée par le juge, motion Rule 29 en attente)

### 37/ · checked · choix :

**37a (simple)** : Crypto #MEV Dark Forest #37: Simple protection: a private RPC like Flashbots Protect. Your wallet sends the transaction privately to block builders, so it never sits in the public mempool where sandwich bots look. #DeFi #CryptoTrading

**37b (tech)** : Crypto #MEV Dark Forest #37: Flashbots Protect RPC: txs skip the public mempool and go to builders directly, so they can't be front-run or sandwiched there. Reverted txs aren't included, so a failed swap costs no gas. #DeFi #CryptoTrading

Source : https://docs.flashbots.net/flashbots-protect/overview

### 38/ · checked · choix :

**38a (simple)** : Crypto #MEV Dark Forest #38: Private RPCs are not magic. With Flashbots Protect, you trust the block builders that get your transaction not to front-run it or leak it to other bots. More builders can mean faster inclusion, but more parties to trust. #DeFi #CryptoTrading

**38b (tech)** : Crypto #MEV Dark Forest #38: Flashbots Protect default: your tx goes only to the Flashbots builder and is dropped if not included within 25 blocks. Sharing with more builders raises the inclusion rate, but you trust each not to frontrun or leak it. #DeFi #CryptoTrading

Source : https://docs.flashbots.net/flashbots-protect/settings-guide + https://docs.flashbots.net/flashbots-protect/quick-start ("you trust them not to frontrun your transaction or disclose it to third-party MEV searchers")

### 39/ · checked · choix :

**39a (simple)** : Crypto #MEV Dark Forest #39: What if the MEV from your trade went back to you? Flashbots' MEV-Share (2023) lets bots back-run your transaction and pays you part of their profit. #DeFi #CryptoTrading

**39b (tech)** : Crypto #MEV Dark Forest #39: MEV-Share, introduced by Flashbots in 2023, is an order flow auction: it reveals selected tx hints to searchers, who bid to back-run; a share of the winning bid is refunded to the user. #DeFi #CryptoTrading

Source : https://docs.flashbots.net/flashbots-mev-share/overview

### 40/ · checked · choix :

**40a (simple)** : Crypto #MEV Dark Forest #40: Another option, MEV Blocker, launched in April 2023 by CoW Swap, Agnostic Relay and Beaver Build. It shields your trade and returns at least 90% of back-run profits to you. #DeFi #CryptoTrading

**40b (tech)** : Crypto #MEV Dark Forest #40: MEV Blocker RPC (launched Apr 5 2023, CoW Swap + Agnostic Relay + Beaver Build): searchers bid to back-run your tx; ≥90% of the winning bid goes back to the user, the rest to the validator. #DeFi #CryptoTrading

Source : https://www.theblock.co/post/225023/ethereum-projects-come-together-on-mev-blocker-rpc-launch-to-share-profits-with-users

### 41/ · checked · choix :

**41a (simple)** : Crypto #MEV Dark Forest #41: CoW Swap groups trades into batches settled at one price per token. If everyone gets the same price in a batch, there is no "before you" for a bot to jump into. #DeFi #CryptoTrading

**41b (tech)** : Crypto #MEV Dark Forest #41: CoW Protocol batch auctions: orders are signed intents, solvers compete to settle a batch, and all trades of a pair in that batch clear at a uniform price, so in-batch ordering gives no edge. #DeFi #CryptoTrading

Source : https://docs.cow.fi/cow-protocol/concepts/benefits/mev-protection

### 42/ · checked · choix :

**42a (simple)** : Crypto #MEV Dark Forest #42: In Dec 2024, Flashbots, Nethermind and beaverbuild announced BuilderNet: a block builder run by several operators together, to fight the centralization of building. #DeFi #CryptoTrading

**42b (tech)** : Crypto #MEV Dark Forest #42: BuilderNet (Dec 2024, Flashbots + Nethermind + beaverbuild): a multi-operator builder running inside trusted execution environments (TEEs), so order flow is processed without being exposed to operators. #DeFi #CryptoTrading

Source : https://blockworks.co/news/flashbots-block-building-network-mev

### 43/ · checked · choix :

**43a (simple)** : Crypto #MEV Dark Forest #43: Ethereum plans to build proposer-builder separation into the protocol itself, so validators no longer need to trust relays. It is the headline feature of the Glamsterdam upgrade. #DeFi #CryptoTrading

**43b (tech)** : Crypto #MEV Dark Forest #43: EIP-7732 (ePBS) enshrines proposer-builder separation in consensus, removing the trusted relay from the auction. It was chosen as the consensus-layer headliner of the Glamsterdam upgrade. #DeFi #CryptoTrading

Source : https://etherworld.co/eip-7732-epbs-selected-as-glamsterdam-headliner/

### 44/ · checked · choix :

**44a (simple)** : Crypto #MEV Dark Forest #44: Against censorship, Ethereum researchers propose "inclusion lists": several validators each pick transactions that the next block must include, so one builder can't quietly leave them out. #DeFi #CryptoTrading

**44b (tech)** : Crypto #MEV Dark Forest #44: FOCIL (EIP-7805, fork-choice enforced inclusion lists): a committee of validators publishes inclusion lists each slot; attesters only vote for a block that includes those txs. Censorship resistance via fork choice. #DeFi #CryptoTrading

Source : https://eips.ethereum.org/EIPS/eip-7805

### 45/ · checked · choix :

**45a (simple)** : Crypto #MEV Dark Forest #45: One research idea: burn the MEV. Builders' bids would be destroyed instead of paid to the validator, so the value goes to all ETH holders. It is a proposal, not live. #DeFi #CryptoTrading

**45b (tech)** : Crypto #MEV Dark Forest #45: MEV burn (Justin Drake, ethresear.ch, 2023): attesters set a floor from observed builder bids, and that part of the winning bid is burned like the EIP-1559 base fee. Research proposal, not scheduled. #DeFi #CryptoTrading

Source : https://ethresear.ch/t/mev-burn-a-simple-design/15590

### 46/ · checked · choix :

**46a (simple)** : Crypto #MEV Dark Forest #46: What if nobody could read your transaction until its place in the block is fixed? Since July 2024, Gnosis Chain offers an encrypted mempool built with Shutter. #DeFi #CryptoTrading

**46b (tech)** : Crypto #MEV Dark Forest #46: Shutterized Gnosis Chain (July 2024): txs are threshold-encrypted; a set of keypers releases the decryption key only after ordering is committed, so front-running by content is not possible. #DeFi #CryptoTrading

Source : https://www.theblock.co/post/305028/gnosis-shutter-mev

### 47/ · checked · choix :

**47a (simple)** : Crypto #MEV Dark Forest #47: On many layer 2s, one sequencer orders all transactions. Arbitrum added Timeboost in April 2025: bots can buy a short head start, and the money goes to the chain's DAO, not a validator. #DeFi #CryptoTrading

**47b (tech)** : Crypto #MEV Dark Forest #47: Arbitrum Timeboost (Arbitrum One, April 2025): a sealed-bid second-price auction sells a 60-second express lane; other txs are delayed by 200 ms while a controller holds the round. #DeFi #CryptoTrading

Source : https://docs.arbitrum.io/how-arbitrum-works/timeboost/timeboost-faq

### 48/ · checked · choix :

**48a (simple)** : Crypto #MEV Dark Forest #48: Solana works differently: there is no classic public mempool. Transactions go straight to the next block leaders, and bots compete through Jito's bundle auctions with tips. #DeFi #CryptoTrading

**48b (tech)** : Crypto #MEV Dark Forest #48: Solana forwards txs directly to upcoming leaders (no global mempool). Jito's validator client adds an off-chain block engine where searchers send bundles with tips, paid to validators who run it and their stakers. #DeFi #CryptoTrading

Source : https://solana.com/news/gulf-stream--solana-s-mempool-less-transaction-forwarding-protocol + https://docs.jito.wtf/lowlatencytxnsend/

### 49/ · checked · choix :

**49a (simple)** : Crypto #MEV Dark Forest #49: Jito did run a mempool on Solana, where bots could see pending transactions. On March 8, 2024 it shut it down because it was mainly used for sandwich attacks. #DeFi #CryptoTrading

**49b (tech)** : Crypto #MEV Dark Forest #49: March 8 2024: Jito Labs suspended its Solana mempool stream, saying anti-sandwich fixes had become "a cat-and-mouse game with MEV searchers" with no engineering solution in sight. #DeFi #CryptoTrading

Source : https://www.coindesk.com/business/2024/03/08/solana-client-developer-jito-announces-end-of-mempool-function

### 50/ · checked · choix :

**50a (simple)** : Crypto #MEV Dark Forest #50: Bitcoin has a mempool too. Since Bitcoin Core 28.0 (Oct 2024), nodes by default accept a replacement for any unconfirmed transaction that pays a higher fee. #DeFi #CryptoTrading

**50b (tech)** : Crypto #MEV Dark Forest #50: Bitcoin Core 28.0 (Oct 2 2024) changed -mempoolfullrbf default from 0 to 1: full replace-by-fee, so nodes relay replacements even without the BIP 125 opt-in signal. #DeFi #CryptoTrading

Source : https://bitcoincore.org/en/releases/28.0/

### 51/ · checked · choix :

**51a (simple)** : Crypto #MEV Dark Forest #51: Lesson from full-RBF: a Bitcoin payment with zero confirmations is a promise, not a payment. Until it is in a block, the sender can replace it with a higher-fee one. #DeFi #CryptoTrading

**51b (tech)** : Crypto #MEV Dark Forest #51: With full-RBF default in Bitcoin Core 28.0, merchants can't rely on the opt-in RBF flag to accept 0-conf: any unconfirmed tx may be double-spent by a higher-fee conflicting tx. #DeFi #CryptoTrading

Source : https://bitcoincore.org/en/releases/28.0/

### 52/ · checked · choix :

**52a (simple)** : Crypto #MEV Dark Forest #52: Recap: your pending transaction is public, bots profit from its order, a few builders assemble most blocks, and private RPCs or batch auctions can protect you. Follow for more crypto deep dives. #DeFi #CryptoTrading

**52b (tech)** : Crypto #MEV Dark Forest #52: Recap: MEV = value from ordering txs. Sandwiches, arbs, liquidations; PGAs to Flashbots bundles to MEV-Boost; builder centralization; ePBS, encrypted mempools, OFAs. Follow for the next series. #DeFi #CryptoTrading

Source : https://ethereum.org/en/developers/docs/mev/
