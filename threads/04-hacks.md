# Thread : Anatomy of a crypto heist

- Sujet : 4. Les grands hacks, expliqués techniquement
- Angle : pas le montant volé, mais la ligne de code ou la clé qui a lâché.
- Préfixe : `Crypto #Hack Autopsy #N:`
- Hashtags fixes : #CyberSecurity #InfoSec
- Statut : draft

Notes : montants en dollars = valeur au moment du hack. Les attributions (Lazarus, TraderTraitor) ne sont citées que lorsque le FBI l'a fait. Aucun mode opératoire détaillé, seulement le principe. Statut "checked" = chiffres vérifiés contre la source notée ; "draft" = à revérifier avant publication.

---

### 1/ · checked · choix :

**1a (simple)** : Crypto #Hack Autopsy #1: The biggest crypto heists rarely "break" cryptography. They exploit one forgotten check, one stolen key, one fake screen. One hack a day, explained: not how much was taken, but what failed. #CyberSecurity #InfoSec

**1b (tech)** : Crypto #Hack Autopsy #1: From The DAO (2016) to Bybit (about $1.5B, 2025), the root cause is almost never broken math: a reentrant call, an unchecked account, a leaked signer key, a tampered UI. Daily autopsies of what actually failed. #CyberSecurity #InfoSec

Source : https://www.ic3.gov/PSA/2025/PSA250226

### 2/ · checked · choix :

**2a (simple)** : Crypto #Hack Autopsy #2: The DAO, June 17, 2016. Its withdraw function sent ETH before updating the user's balance. The attacker kept asking again mid-payment and drained about 3.6M ETH. #CyberSecurity #InfoSec

**2b (tech)** : Crypto #Hack Autopsy #2: The DAO (June 17, 2016): a reentrancy bug. The split/withdraw logic made an external call before zeroing the sender's balance, so a malicious contract re-entered it again and again. About 3.64M ETH moved out. #CyberSecurity #InfoSec

Source : https://cryptobriefing.com/dao-exploit-10th-anniversary-ethereum/

### 3/ · checked · choix :

**3a (simple)** : Crypto #Hack Autopsy #3: The DAO's stolen ETH sat in a "child DAO" locked for 28 days. That delay gave Ethereum time to hard fork on July 20, 2016 and undo the theft. Those who refused kept the old chain: Ethereum Classic. #CyberSecurity #InfoSec

**3b (tech)** : Crypto #Hack Autopsy #3: The DAO's split mechanism moved the drained ETH into a child DAO with a 28-day withdrawal lock. Ethereum used that window to hard fork (July 20, 2016) and return the funds. The unforked chain lives on as Ethereum Classic. #CyberSecurity #InfoSec

Source : https://cryptobriefing.com/dao-exploit-10th-anniversary-ethereum/

### 4/ · checked · choix :

**4a (simple)** : Crypto #Hack Autopsy #4: In August 2010, one Bitcoin transaction created about 184 billion BTC out of nothing. The code added up outputs without checking for overflow. Satoshi shipped a fix within hours and the bad block was orphaned. #CyberSecurity #InfoSec

**4b (tech)** : Crypto #Hack Autopsy #4: Value overflow incident (CVE-2010-5139), block 74638: two huge outputs overflowed the 64-bit sum check, so about 184 billion BTC looked valid. A patched client rejected it and the honest chain overtook the bad one. #CyberSecurity #InfoSec

Source : https://en.bitcoin.it/wiki/CVE-2010-5139

### 5/ · checked · choix :

**5a (simple)** : Crypto #Hack Autopsy #5: Mt. Gox's slow bleed started in 2011. In 2023 the US DOJ charged two Russian nationals with hacking the exchange's wallet server and laundering about 647,000 stolen BTC. #CyberSecurity #InfoSec

**5b (tech)** : Crypto #Hack Autopsy #5: Mt. Gox wasn't one hack but a leak. Per a 2023 DOJ indictment, attackers got into the server holding Mt. Gox's wallets around Sept 2011 and siphoned about 647,000 BTC through 2014, laundered via accounts at other exchanges. #CyberSecurity #InfoSec

Source : https://www.justice.gov/archives/opa/pr/russian-nationals-charged-hacking-one-cryptocurrency-exchange-and-illicitly-operating-another

### 6/ · checked · choix :

**6a (simple)** : Crypto #Hack Autopsy #6: Bitfinex, 2016: 119,754 BTC stolen. Six years later the US recovered more than 94,000 of them, worth over $3.6B at the time. Blockchains never forget: the coins stayed traceable. #CyberSecurity #InfoSec

**6b (tech)** : Crypto #Hack Autopsy #6: Bitfinex (2016): per the DOJ, a hacker breached the exchange's systems and initiated 2,000+ unauthorized transactions, moving 119,754 BTC. In Feb 2022 the DOJ said agents had recovered more than 94,000 of them. #CyberSecurity #InfoSec

Source : https://www.justice.gov/archives/opa/pr/two-arrested-alleged-conspiracy-launder-45-billion-stolen-cryptocurrency

### 7/ · checked · choix :

**7a (simple)** : Crypto #Hack Autopsy #7: July 19, 2017: a setup function in Parity's multisig wallet was left open to anyone. An attacker called it on 3 wallets, made himself owner, and walked off with about 153,000 ETH. #CyberSecurity #InfoSec

**7b (tech)** : Crypto #Hack Autopsy #7: Parity multisig (July 19, 2017): initWallet in the shared library had no "already initialized" guard and was reachable through the wallet's fallback delegatecall. Re-init = new owner. About 153,037 ETH taken from 3 wallets. #CyberSecurity #InfoSec

Source : https://www.openzeppelin.com/news/on-the-parity-wallet-multisig-hack-405a8c12e8f7

### 8/ · checked · choix :

**8a (simple)** : Crypto #Hack Autopsy #8: November 6, 2017: a user poking at Parity's wallet code became owner of the shared library, then hit its self-destruct. 587 wallets lost their code. About 513,774 ETH frozen, still stuck today. #CyberSecurity #InfoSec

**8b (tech)** : Crypto #Hack Autopsy #8: Parity, Nov 6, 2017: the fixed library from July was deployed with its owner uninitialized. "devops199" called init, became owner, then kill(). 587 wallets lost their logic; 513,774.16 ETH frozen. #CyberSecurity #InfoSec

Source : https://medium.com/paritytech/a-postmortem-on-the-parity-multi-sig-library-self-destruct-63daca3a4cf7

### 9/ · checked · choix :

**9a (simple)** : Crypto #Hack Autopsy #9: Why did one deleted contract freeze 587 wallets? To save fees, every Parity wallet borrowed its logic from one shared library. Delete the library and the wallets can no longer do anything, not even send. #CyberSecurity #InfoSec

**9b (tech)** : Crypto #Hack Autopsy #9: Parity wallets were thin proxies: each forwarded calls via delegatecall to one library contract. After the library self-destructed, the proxies pointed at empty code. Funds intact, logic gone: frozen, not stolen. #CyberSecurity #InfoSec

Source : https://medium.com/paritytech/a-postmortem-on-the-parity-multi-sig-library-self-destruct-63daca3a4cf7

### 10/ · checked · choix :

**10a (simple)** : Crypto #Hack Autopsy #10: Coincheck, January 2018: about 523M NEM (about $530M) stolen. The coins sat in a hot wallet, online, protected by a single key, with no multisig. One key, one breach, half a billion gone. #CyberSecurity #InfoSec

**10b (tech)** : Crypto #Hack Autopsy #10: Coincheck (Jan 26, 2018) kept nearly all its XEM in one internet-connected hot wallet with a single-signature key, though NEM supports multisig. Attackers who took the key moved about 523M XEM. Coincheck later refunded users. #CyberSecurity #InfoSec

Source : https://cointelegraph.com/news/coincheck-stolen-534-mln-nem-were-stored-on-low-security-hot-wallet

### 11/ · checked · choix :

**11a (simple)** : Crypto #Hack Autopsy #11: Lendf.Me, April 2020: a token called imBTC can run code when it's transferred. The lending app didn't expect that, so the attacker re-entered mid-deposit and inflated his balance. About $25M, later returned. #CyberSecurity #InfoSec

**11b (tech)** : Crypto #Hack Autopsy #11: Lendf.Me (April 19, 2020): imBTC is ERC-777, which calls a sender hook during transfers. The protocol updated balances after the transfer, so the hook re-entered and credited deposits twice. About $25M drained, then returned. #CyberSecurity #InfoSec

Source : https://quantstamp.com/blog/how-the-dforce-hacker-used-reentrancy-to-steal-25-million

### 12/ · checked · choix :

**12a (simple)** : Crypto #Hack Autopsy #12: Uranium Finance, April 2021: a copied exchange code changed a number in one place but not in another. The safety check became 100 times too loose. About $50M drained. One typo. #CyberSecurity #InfoSec

**12b (tech)** : Crypto #Hack Autopsy #12: Uranium (April 2021), a Uniswap V2 fork, scaled balances by 10000 for fees but kept 1000 in the constant-product (k) check. The invariant was off by 100x, so swaps could pull out far more than they put in. About $50M. #CyberSecurity #InfoSec

Source : https://immunefi.com/blog/bug-fix-reviews/hack-analysis-uranium-finance-april-2021/

### 13/ · checked · choix :

**13a (simple)** : Crypto #Hack Autopsy #13: Poly Network, August 2021: about $611M taken across chains, at the time the largest DeFi hack ever. Then the attacker gave almost all of it back. The flaw: a message could rewrite who the bridge trusted. #CyberSecurity #InfoSec

**13b (tech)** : Crypto #Hack Autopsy #13: Poly Network (Aug 10, 2021): a crafted cross-chain message made the bridge's manager contract call its own data contract and replace the "keeper" public keys. With his key installed, the attacker signed withdrawals. About $611M. #CyberSecurity #InfoSec

Source : https://blocksec.com/blog/the-retrospection-of-the-poly-network-hack-from-a-security-researcher-perspective

### 14/ · checked · choix :

**14a (simple)** : Crypto #Hack Autopsy #14: Smart contract functions are called by a 4-byte ID. In Poly Network, the attacker searched for a harmless-looking name whose ID matched a powerful admin function. Same ID, different door. #CyberSecurity #InfoSec

**14b (tech)** : Crypto #Hack Autopsy #14: Poly Network: the bridge built a call from user-chosen method names. The attacker brute-forced a name whose 4-byte keccak selector equals putCurEpochConPubKeyBytes, the keeper-reset function. A selector collision. #CyberSecurity #InfoSec

Source : https://blog.gopluslabs.io/vulnerabilities-cases/smart-contract/hash-collision/poly-network

### 15/ · checked · choix :

**15a (simple)** : Crypto #Hack Autopsy #15: BadgerDAO, Dec 2021: the contracts were fine. Attackers took over the website's Cloudflare account and slipped in a script that asked users for unlimited token permissions. About $120M drained. #CyberSecurity #InfoSec

**15b (tech)** : Crypto #Hack Autopsy #15: BadgerDAO (Dec 2, 2021): a compromised Cloudflare API key let attackers inject JavaScript into the app. It prompted users to sign approve() calls to an attacker address, later used to transferFrom about $120M. #CyberSecurity #InfoSec

Source : https://halborn.com/explained-the-badgerdao-hack-december-2021

### 16/ · checked · choix :

**16a (simple)** : Crypto #Hack Autopsy #16: Qubit, January 2022: the bridge believed deposits that never happened. A "transfer" from a non-existent token didn't fail, it just did nothing, and the bridge counted it as real. About $80M minted. #CyberSecurity #InfoSec

**16b (tech)** : Crypto #Hack Autopsy #16: Qubit QBridge (Jan 2022): the ERC-20 deposit path was fed the zero address as token. A low-level transferFrom to an address with no code returns success, so the event fired and xETH was minted on BSC. About $80M. #CyberSecurity #InfoSec

Source : https://blocksec.com/blog/when-safe-transfer-becomes-unsafe-lessons-from-the-q-bridge-security-incident

### 17/ · checked · choix :

**17a (simple)** : Crypto #Hack Autopsy #17: Wormhole, Feb 2022: the bridge checked signatures using an old function that didn't confirm where its data came from. The attacker fed it fake proof and minted 120,000 wETH on Solana with no ETH behind it. #CyberSecurity #InfoSec

**17b (tech)** : Crypto #Hack Autopsy #17: Wormhole (Feb 2, 2022): verify_signatures used the deprecated load_instruction_at, which doesn't check that the account passed is the real Instructions sysvar. A fake one skipped secp256k1 checks: 120,000 wETH minted. #CyberSecurity #InfoSec

Source : https://ackee.xyz/blog/2022-solana-hacks-explained-wormhole/

### 18/ · checked · choix :

**18a (simple)** : Crypto #Hack Autopsy #18: After Wormhole's 120,000 wETH were minted from nothing, the bridge's tokens were no longer fully backed. Investor Jump Crypto put in 120,000 ETH of its own to restore the backing. #CyberSecurity #InfoSec

**18b (tech)** : Crypto #Hack Autopsy #18: A bridge mint without a lock breaks the 1:1 peg: Solana wETH outnumbered ETH held on Ethereum by 120,000. Jump Crypto, a Wormhole backer, deposited 120,000 ETH to re-collateralize it. #CyberSecurity #InfoSec

Source : https://ackee.xyz/blog/2022-solana-hacks-explained-wormhole/

### 19/ · checked · choix :

**19a (simple)** : Crypto #Hack Autopsy #19: Ronin bridge, March 2022: withdrawals needed approval from 5 of 9 validators. The attacker got 5 keys. No bug in the code: it did exactly what 5 signatures told it to. 173,600 ETH + 25.5M USDC gone. #CyberSecurity #InfoSec

**19b (tech)** : Crypto #Hack Autopsy #19: Ronin (Mar 23, 2022): 5-of-9 validator threshold. 4 keys were run by Sky Mavis, 1 by Axie DAO: one company could almost sign alone. The attacker took all 5 and withdrew 173,600 ETH and 25.5M USDC. #CyberSecurity #InfoSec

Source : https://roninchain.com/blog/posts/back-to-building-ronin-security-breach-6513cc78a5edc1001b03c364

### 20/ · checked · choix :

**20a (simple)** : Crypto #Hack Autopsy #20: The Ronin theft happened on March 23, 2022. Nobody noticed until March 29, six days later. Hundreds of millions left the bridge and no alarm went off. #CyberSecurity #InfoSec

**20b (tech)** : Crypto #Hack Autopsy #20: Ronin's drain was two transactions on Mar 23, 2022, discovered Mar 29. Six days with no alerting on bridge outflows, even though the bridge's reserves had just been emptied. #CyberSecurity #InfoSec

Source : https://roninchain.com/blog/posts/back-to-building-ronin-security-breach-6513cc78a5edc1001b03c364

### 21/ · checked · choix :

**21a (simple)** : Crypto #Hack Autopsy #21: How did Ronin's attacker get a 5th key? In Nov 2021, Axie DAO let Sky Mavis sign on its behalf to ease traffic. The program ended in December, but the permission was never removed. #CyberSecurity #InfoSec

**21b (tech)** : Crypto #Hack Autopsy #21: Ronin's 5th signature: Axie DAO had allowlisted Sky Mavis to sign for it (Nov 2021) for a free-transaction program. Stopped in Dec, never revoked. The attacker used the gas-free RPC node as a backdoor to get it. #CyberSecurity #InfoSec

Source : https://roninchain.com/blog/posts/back-to-building-ronin-security-breach-6513cc78a5edc1001b03c364

### 22/ · checked · choix :

**22a (simple)** : Crypto #Hack Autopsy #22: Ronin's entry point was a person, not code: a Sky Mavis employee was hit by targeted phishing on social channels. In April 2022 the FBI attributed the theft to North Korea's Lazarus Group. #CyberSecurity #InfoSec

**22b (tech)** : Crypto #Hack Autopsy #22: Ronin: spear-phishing compromised a Sky Mavis employee, giving access to IT infrastructure and the validator nodes. The FBI named Lazarus Group (DPRK) in April 2022 and Treasury sanctioned the receiving address. #CyberSecurity #InfoSec

Source : https://roninchain.com/blog/posts/back-to-building-ronin-security-breach-6513cc78a5edc1001b03c364

### 23/ · checked · choix :

**23a (simple)** : Crypto #Hack Autopsy #23: Cashio, March 2022: a stablecoin minted new coins against collateral. It checked the collateral looked right, but not that it was real. Fake collateral in, real coins out: an "infinite mint" of about $50M. #CyberSecurity #InfoSec

**23b (tech)** : Crypto #Hack Autopsy #23: Cashio (Solana, Mar 23, 2022): the mint path didn't validate the mint field of the Saber LP collateral account, so attacker-made fake accounts passed the chain of checks. CASH minted from nothing; about $48M-52M drained. #CyberSecurity #InfoSec

Source : https://ackee.xyz/blog/2022-solana-hacks-explained-cashio/

### 24/ · checked · choix :

**24a (simple)** : Crypto #Hack Autopsy #24: Beanstalk, April 2022: the attacker borrowed over $1B for a few seconds, used it to win a governance vote, and passed a proposal sending the treasury to himself. Losses: about $182M. #CyberSecurity #InfoSec

**24b (tech)** : Crypto #Hack Autopsy #24: Beanstalk (Apr 17, 2022): a $1B+ flash loan bought deposits that counted as votes in the same transaction, reaching 2/3. emergencyCommit then executed BIP-18, proposed a day earlier, draining about $182M. #CyberSecurity #InfoSec

Source : https://medium.com/immunefi/hack-analysis-beanstalk-governance-attack-april-2022-f42788fc821e

### 25/ · checked · choix :

**25a (simple)** : Crypto #Hack Autopsy #25: Beanstalk's attacker filed two proposals: BIP-18, which drained the protocol, and BIP-19, which sent $250k in BEAN to a Ukraine donation address. He kept about $77M. #CyberSecurity #InfoSec

**25b (tech)** : Crypto #Hack Autopsy #25: Beanstalk's governance had a 1-day delay before emergency execution, but no snapshot of past voting power. Flash-borrowed power counted. The attacker netted about $77M in non-Bean assets; total losses about $182M. #CyberSecurity #InfoSec

Source : https://medium.com/immunefi/hack-analysis-beanstalk-governance-attack-april-2022-f42788fc821e

### 26/ · checked · choix :

**26a (simple)** : Crypto #Hack Autopsy #26: Harmony's Horizon bridge, June 23, 2022: per Harmony, the attacker compromised at least 2 of the validator keys that approved withdrawals, and that was enough to empty the bridge. Steal a couple of keys, own the bridge. #CyberSecurity #InfoSec

**26b (tech)** : Crypto #Hack Autopsy #26: Horizon bridge (June 23, 2022): Harmony says the attacker compromised at least 2 bridge validator private keys, likely after phishing a developer, and used them to move 14 assets on Ethereum plus BNB on BSC. #CyberSecurity #InfoSec

Source : https://medium.com/harmony-one/summary-of-the-harmony-horizon-bridge-incident-f9bd87c0c68e

### 27/ · checked · choix :

**27a (simple)** : Crypto #Hack Autopsy #27: Nomad bridge, Aug 2022: a routine upgrade accidentally marked "empty" as a trusted value. Result: every message counted as valid, even ones never sent. About $190M drained. #CyberSecurity #InfoSec

**27b (tech)** : Crypto #Hack Autopsy #27: Nomad (Aug 1, 2022): an upgrade initialized the zero root (0x00) as trusted. Unproven messages map to that default, so any withdrawal passed process(). Improper initialization, about $190M lost. #CyberSecurity #InfoSec

Source : https://www.theblock.co/post/160851/nomads-190-million-bridge-exploit-drew-hacking-feeding-frenzy-of-300-addresses

### 28/ · checked · choix :

**28a (simple)** : Crypto #Hack Autopsy #28: Nomad was the first "crowd-sourced" hack. No skill needed: people copied the first attack, put in their own address, and resent it. Over 300 addresses joined within about an hour. #CyberSecurity #InfoSec

**28b (tech)** : Crypto #Hack Autopsy #28: Nomad: since any message passed, a replayed calldata with a swapped recipient worked. PeckShield counted 300+ addresses; 41 took about $152M (about 80%). At least 6 white hats took about $8.2M to return. #CyberSecurity #InfoSec

Source : https://www.theblock.co/post/160851/nomads-190-million-bridge-exploit-drew-hacking-feeding-frenzy-of-300-addresses

### 29/ · checked · choix :

**29a (simple)** : Crypto #Hack Autopsy #29: Wintermute, Sept 2022: about $160M lost. Its admin wallet had a "vanity" address made with a tool called Profanity. That tool's randomness was weak, so the private key could be recomputed. #CyberSecurity #InfoSec

**29b (tech)** : Crypto #Hack Autopsy #29: Wintermute (Sept 20, 2022): an admin hot wallet with a 0x0000000 vanity address from Profanity. Days earlier, 1inch had shown Profanity keys can be brute-forced back from the public key. About $160M taken. #CyberSecurity #InfoSec

Source : https://www.theblock.co/post/171192/experts-blame-a-vanity-address-bug-for-wintermutes-160-million-hack

### 30/ · checked · choix :

**30a (simple)** : Crypto #Hack Autopsy #30: A private key should come from a number so big no one can guess it. Profanity started from a seed of only about 4 billion options, small enough for an attacker to precompute every one of them. #CyberSecurity #InfoSec

**30b (tech)** : Crypto #Hack Autopsy #30: Profanity seeded its key search with only 32 random bits (about 4.3B seeds), then stepped with no one-way function. Precompute all 2^32 seed keys, match them against a vanity public key, recover the private key. #CyberSecurity #InfoSec

Source : https://support.safeheron.com/blog/insights/safeheron-originals/a-deep-dive-of-how-profanity-caused-wintermute-to-lose-usd160m

### 31/ · checked · choix :

**31a (simple)** : Crypto #Hack Autopsy #31: BNB Chain bridge, Oct 2022: the attacker forged a proof and minted 2M BNB. Validators halted the whole chain, so only a part, about $100M, got out to other chains. #CyberSecurity #InfoSec

**31b (tech)** : Crypto #Hack Autopsy #31: BSC Token Hub (Oct 6, 2022): a bug in IAVL Merkle proof verification let the attacker forge a proof for a fake cross-chain message and mint 2M BNB. The chain was paused; about $100M+ left before it stopped. #CyberSecurity #InfoSec

Source : https://immunefi.com/blog/bug-fix-reviews/hack-analysis-binance-bridge-october-2022/

### 32/ · checked · choix :

**32a (simple)** : Crypto #Hack Autopsy #32: Mango Markets, Oct 2022: no code bug at all. The attacker pumped the price of a thinly traded token, used his now "valuable" position as collateral, and borrowed about $110M. #CyberSecurity #InfoSec

**32b (tech)** : Crypto #Hack Autopsy #32: Mango Markets (Oct 11, 2022): price-oracle manipulation. Spot MNGO on thin markets was pushed up sharply, marking a large MNGO-PERP position deep in profit; that unrealized PnL backed about $110M of borrows. #CyberSecurity #InfoSec

Source : https://cointelegraph.com/news/how-low-liquidity-led-to-mango-markets-losing-over-116-million

### 33/ · checked · choix :

**33a (simple)** : Crypto #Hack Autopsy #33: Euler Finance, March 2023: a "donate" function let users give funds to the protocol without checking if they still had enough collateral. The attacker made himself insolvent on purpose, then cashed in. About $197M. #CyberSecurity #InfoSec

**33b (tech)** : Crypto #Hack Autopsy #33: Euler (Mar 13, 2023): donateToReserves() skipped the checkLiquidity() health check. Flash loan, leveraged position, donate eTokens to go insolvent, self-liquidate at a discount. About $197M. #CyberSecurity #InfoSec

Source : https://www.certik.com/blog/euler-finance-incident-analysis

### 34/ · checked · choix :

**34a (simple)** : Crypto #Hack Autopsy #34: The Euler story ended oddly: after weeks of on-chain messages with the team, the attacker returned the recoverable funds. A rare case where nearly everything came back. #CyberSecurity #InfoSec

**34b (tech)** : Crypto #Hack Autopsy #34: Euler's exploiter returned funds in tranches over March-April 2023; a final $31M in early April marked the end of "recoverable funds", and Euler then opened redemptions for users. #CyberSecurity #InfoSec

Source : https://www.theblock.co/post/224705/euler-hacker-returns-funds

### 35/ · checked · choix :

**35a (simple)** : Crypto #Hack Autopsy #35: Curve, July 2023: the pools' code had a lock against re-entering mid-action. But the compiler that turned it into bytecode had a bug that broke the lock. Several pools drained, about $60M+. #CyberSecurity #InfoSec

**35b (tech)** : Crypto #Hack Autopsy #35: Curve (July 30, 2023): Vyper 0.2.15, 0.2.16 and 0.3.0 compiled @nonreentrant locks wrongly. A raw_call sending ETH let attackers re-enter between remove and add liquidity. Hit: pETH, msETH, alETH and CRV/ETH pools. #CyberSecurity #InfoSec

Source : https://hackmd.io/%40LlamaRisk/BJzSKHNjn

### 36/ · checked · choix :

**36a (simple)** : Crypto #Hack Autopsy #36: The Curve hack's lesson: you can audit the code and still be wrong, if the tool that compiles it is broken. The bug had even been fixed in a later version, by accident, without anyone flagging it. #CyberSecurity #InfoSec

**36b (tech)** : Crypto #Hack Autopsy #36: The Vyper reentrancy bug came in via one PR and was fixed in 0.3.1 almost incidentally, with no advisory. Contracts compiled with the 3 bad versions kept shipping a broken lock. Audits review source, not bytecode. #CyberSecurity #InfoSec

Source : https://hackmd.io/%40LlamaRisk/BJzSKHNjn

### 37/ · checked · choix :

**37a (simple)** : Crypto #Hack Autopsy #37: Ledger, Dec 14, 2023: a former employee was phished, and the attacker used the access to publish a poisoned version of a code library used by many crypto apps. Users saw a drainer. About $600K lost. #CyberSecurity #InfoSec

**37b (tech)** : Crypto #Hack Autopsy #37: Ledger Connect Kit (Dec 14, 2023): a phished ex-employee's npm access was used to publish malicious versions. Dapps that loaded the kit from a CDN served a wallet drainer for a few hours. About $600K stolen. #CyberSecurity #InfoSec

Source : https://www.bleepingcomputer.com/news/security/ledger-dapp-supply-chain-attack-steals-600k-from-crypto-wallets/

### 38/ · checked · choix :

**38a (simple)** : Crypto #Hack Autopsy #38: Orbit Bridge, New Year's Eve 2023: about $81.5M in 5 assets left its Ethereum vault. The odd part: Orbit says it was neither a smart-contract bug nor a stolen validator key, and left the cause under investigation. #CyberSecurity #InfoSec

**38b (tech)** : Crypto #Hack Autopsy #38: Orbit Bridge (Dec 31, 2023 ≈21:00 UTC): ≈$81.5M in ETH, WBTC, USDT, USDC and DAI withdrawn from its Ethereum vault. Orbit's statement: not a contract vulnerability, not validator key theft; investigation ongoing. #CyberSecurity #InfoSec

Source : https://medium.com/orbit-chain/official-statement-regarding-orbit-bridge-exploit-551928f3dc52 (UTC timing: https://rekt.news/orbit-bridge-rekt)

### 39/ · checked · choix :

**39a (simple)** : Crypto #Hack Autopsy #39: DMM Bitcoin, May 2024: 4,502.9 BTC (about $308M) stolen. Per the FBI, it began with a fake LinkedIn recruiter sending a "coding test" to an employee of DMM's wallet provider. #CyberSecurity #InfoSec

**39b (tech)** : Crypto #Hack Autopsy #39: DMM Bitcoin (May 31, 2024): FBI says TraderTraitor (DPRK) posed as a recruiter and sent a Ginco employee a malicious Python script as a pre-employment test, then hijacked a legit transaction request. 4,502.9 BTC. #CyberSecurity #InfoSec

Source : https://cryptoslate.com/fbi-reveals-north-korea-used-linkedin-to-steal-305-million-from-japans-dmm-bitcoin/ ; https://www.bleepingcomputer.com/news/security/fbi-links-north-korean-hackers-to-308-million-crypto-heist

### 40/ · checked · choix :

**40a (simple)** : Crypto #Hack Autopsy #40: WazirX, July 18, 2024: over $230M taken from a multisig wallet. The signers approved what looked like a normal transaction. Per WazirX, what they really signed handed the wallet's control to the attacker. #CyberSecurity #InfoSec

**40b (tech)** : Crypto #Hack Autopsy #40: WazirX (July 18, 2024): a Gnosis Safe needing 3 WazirX signers plus 1 from custody partner Liminal. WazirX says the signed payload differed from what Liminal's interface showed and gave control to the attacker. Over $230M. #CyberSecurity #InfoSec

Source : https://wazirx.com/blog/preliminary-report-cyber-attack-on-wazirx-multisig-wallet/

### 41/ · checked · choix :

**41a (simple)** : Crypto #Hack Autopsy #41: Radiant Capital, Oct 2024: malware on 3 developers' computers showed a normal transaction on screen while sending a malicious one to their hardware wallets. 3 signatures was all it took. About $50M. #CyberSecurity #InfoSec

**41b (tech)** : Crypto #Hack Autopsy #41: Radiant (Oct 16, 2024): 3/11 multisig on Arbitrum. Malware showed benign Safe data, faked an error to get re-signs, and collected 3 blind-signed approvals for transferOwnership of the lending pool provider. About $50M. #CyberSecurity #InfoSec

Source : https://medium.com/@RadiantCapital/radiant-post-mortem-fecd6cd38081

### 42/ · checked · choix :

**42a (simple)** : Crypto #Hack Autopsy #42: Bybit, Feb 21, 2025: about 401,000 ETH (about $1.5B) taken from one cold wallet. The largest crypto theft ever recorded. And the exchange's own keys were never stolen. #CyberSecurity #InfoSec

**42b (tech)** : Crypto #Hack Autopsy #42: Bybit (Feb 21, 2025): about 401,347 ETH drained from a Safe multisig cold wallet, the largest crypto theft on record. Every signature was genuine: the signers were tricked into approving the wrong payload. #CyberSecurity #InfoSec

Source : https://www.nccgroup.com/research/in-depth-technical-analysis-of-the-bybit-hack/

### 43/ · checked · choix :

**43a (simple)** : Crypto #Hack Autopsy #43: How Bybit was fooled: attackers hacked a developer of Safe, the wallet app Bybit used, and changed the app's web code. For Bybit only, it showed a routine transfer while preparing a very different one. #CyberSecurity #InfoSec

**43b (tech)** : Crypto #Hack Autopsy #43: Bybit: a compromised Safe{Wallet} developer machine led to malicious JavaScript in app.safe.global. It only triggered for Bybit-linked addresses, swapping tx data at signing while the UI showed the expected transfer. #CyberSecurity #InfoSec

Source : https://www.nccgroup.com/research/in-depth-technical-analysis-of-the-bybit-hack/

### 44/ · checked · choix :

**44a (simple)** : Crypto #Hack Autopsy #44: What did Bybit's signers actually approve? Not a transfer, but a swap of the wallet's "brain": the code that decides who can move funds was replaced by the attacker's. Draining was then one step away. #CyberSecurity #InfoSec

**44b (tech)** : Crypto #Hack Autopsy #44: Bybit payload: operation=1 (delegatecall) to an attacker contract dressed as an ERC-20 transfer. Running in the proxy's context, it overwrote storage slot 0, the Safe's implementation address. #CyberSecurity #InfoSec

Source : https://www.nccgroup.com/research/in-depth-technical-analysis-of-the-bybit-hack/

### 45/ · checked · choix :

**45a (simple)** : Crypto #Hack Autopsy #45: Five days after the Bybit heist, the FBI publicly named North Korea as responsible, under the label "TraderTraitor", and asked the industry to block addresses tied to the theft. #CyberSecurity #InfoSec

**45b (tech)** : Crypto #Hack Autopsy #45: FBI PSA I-022625-PSA (Feb 26, 2025) attributed the about $1.5B Bybit theft to the DPRK, tracked as TraderTraitor, and published attacker addresses for nodes, exchanges and bridges to block. #CyberSecurity #InfoSec

Source : https://www.ic3.gov/PSA/2025/PSA250226

### 46/ · checked · choix :

**46a (simple)** : Crypto #Hack Autopsy #46: Cetus, May 2025, on Sui: a math library checked for overflow with the wrong limit. A huge number slipped past, the result wrapped around, and tiny deposits bought huge liquidity. About $223M. #CyberSecurity #InfoSec

**46b (tech)** : Crypto #Hack Autopsy #46: Cetus (May 22, 2025): checked_shlw in the integer-mate u256 library guarded a << 64 shift with a wrong bound. Overflowed bits were silently truncated, so a near-zero deposit minted massive CLMM liquidity. About $223M. #CyberSecurity #InfoSec

Source : https://www.cyfrin.io/blog/inside-the-223m-cetus-exploit-root-cause-and-impact-analysis

### 47/ · checked · choix :

**47a (simple)** : Crypto #Hack Autopsy #47: After the Cetus hack, Sui validators blocked the attacker's addresses, keeping about $162M of the $223M from leaving. Funds saved, but it reopened an old debate: who can freeze coins on a chain? #CyberSecurity #InfoSec

**47b (tech)** : Crypto #Hack Autopsy #47: Cetus: about $60M was bridged to Ethereum fast; about $162M stayed on Sui, where validators coordinated to ignore transactions from attacker addresses. Effective, and a live test of validator-level censorship. #CyberSecurity #InfoSec

Source : https://www.cyfrin.io/blog/inside-the-223m-cetus-exploit-root-cause-and-impact-analysis

### 48/ · checked · choix :

**48a (simple)** : Crypto #Hack Autopsy #48: Balancer, Nov 3, 2025: about $128M drained by rounding. Each swap rounded a tiny bit in the wrong direction; chained many times in one transaction, the crumbs added up to a skewed pool price. #CyberSecurity #InfoSec

**48b (tech)** : Crypto #Hack Autopsy #48: Balancer V2 (Nov 3, 2025): a rounding error in Composable Stable Pool math, compounded across many steps inside one batchSwap, distorted the pool invariant and BPT price. About $128M across several chains. #CyberSecurity #InfoSec

Source : https://research.checkpoint.com/2025/how-an-attacker-drained-128m-from-balancer-through-rounding-error-exploitation/

### 49/ · checked · choix :

**49a (simple)** : Crypto #Hack Autopsy #49: Ethereum Classic, August 2020: attackers gathered enough mining power to rewrite the chain's recent history, over 7,000 blocks in one case, undoing payments. Three such attacks in one month. #CyberSecurity #InfoSec

**49b (tech)** : Crypto #Hack Autopsy #49: ETC (Aug 2020): 51% attacks. An attacker mined a longer private chain, then released it; one reorg replaced 7,000+ blocks, enabling double spends. Three attacks that month. #CyberSecurity #InfoSec

Source : https://www.nasdaq.com/articles/ethereum-classic-hit-by-third-51-attack-in-a-month-2020-08-29

### 50/ · checked · choix :

**50a (simple)** : Crypto #Hack Autopsy #50: Spot the pattern: at Ronin the attacker got 5 of 9 validator keys, at Harmony at least 2 of 4. Bridges holding huge sums were guarded by a handful of keys. Steal enough of them and the code obeys. #CyberSecurity #InfoSec

**50b (tech)** : Crypto #Hack Autopsy #50: Ronin (5 of 9 validator keys taken), Horizon (at least 2 of 4): in both, the contracts worked as designed. Security collapsed to how a few signer keys were stored. Signers run by one team are a single point of failure. #CyberSecurity #InfoSec

Source : https://roninchain.com/blog/posts/back-to-building-ronin-security-breach-6513cc78a5edc1001b03c364 + https://medium.com/harmony-one/summary-of-the-harmony-horizon-bridge-incident-f9bd87c0c68e

### 51/ · checked · choix :

**51a (simple)** : Crypto #Hack Autopsy #51: Another pattern: WazirX, Radiant, Bybit. Hardware wallets, multisig, all in place. Attackers simply changed what signers saw on screen. If you can't read what you sign, you're trusting the screen. #CyberSecurity #InfoSec

**51b (tech)** : Crypto #Hack Autopsy #51: WazirX, Radiant, Bybit: each multisig got valid signatures over a malicious payload, because the UI was compromised and devices blind-signed raw calldata. Fix: decode and verify the tx hash on an independent device. #CyberSecurity #InfoSec

Source : https://medium.com/@RadiantCapital/radiant-post-mortem-fecd6cd38081

### 52/ · checked · choix :

**52a (simple)** : Crypto #Hack Autopsy #52: Recap: a function called twice, an unchecked input, a forgotten permission, a bad compiler, a fake screen. The code is rarely broken, the assumptions are. Follow for more crypto security autopsies. #CyberSecurity #InfoSec

**52b (tech)** : Crypto #Hack Autopsy #52: Dozens of heists, a few root causes: reentrancy, missing validation, bad init, oracle/governance games, rounding, toolchain bugs, key theft, UI tampering. Learn them once, spot them everywhere. Follow for more. #CyberSecurity #InfoSec

Source : https://www.ic3.gov/PSA/2025/PSA250226
