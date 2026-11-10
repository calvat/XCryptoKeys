# Thread : Inside Pactus

- Sujet : Pactus (série de lancement du compte)
- Angle : comment Pactus fonctionne, vérifié dans le code de `main`
- Statut : checked (vérifiés contre le code), à publier

English, 280 characters max, one technical point per post. Each post has two versions: "Na (simple)" for a general audience and "Nb (tech)". Pick one per number. "≈" values are arithmetic from the constants (block time = 10 s). Figures are protocol defaults, which match the mainnet genesis unless a post says otherwise.

Things to know before posting:
- Series 5 (consensus): cross-check before posting.
- Post 56 (anchors, PIP-50) needs protocol v5, which activates by a 75 % stake signal. Check that it is active on mainnet before posting it as a live feature.
- Post 3: rewards are paid from the 21M PAC treasury, so at the coded rates it is empty around block 44M. The 0.25 tier never reaches 56M and the 0.125 tier is never reached. Don't add "then 0.125".


## Series 1 — Chain basics & economics

1a. (simple) Inside #Pactus #1: Pactus has a fixed supply: 42M PAC, all created at launch. Nothing new is ever minted. Half sits in a treasury that pays out block rewards over time. #PAC @PactusChain

1b. (tech) Inside #Pactus #1: Pactus has a fixed supply of 42,000,000 PAC, all allocated in the genesis file. Half (21M) sits in the treasury, the all-zero address, and is paid out as block rewards. No minting: rewards come from this pre-allocated pot. #PAC @PactusChain

2a. (simple) Inside #Pactus #2: A new Pactus block every 10 seconds, on a steady clock: block times always land on the same 10-second marks of each minute. #PAC @PactusChain

2b. (tech) Inside #Pactus #2: Pactus block time is 10 seconds. A custom genesis is rejected if its interval doesn't divide 60, and block times must have a seconds field that's a multiple of the interval, so the 10 s slots line up the same way every minute. #PAC @PactusChain

3a. (simple) Inside #Pactus #3: Block rewards shrink over time: 1 PAC per block at first, then 0.5, then 0.25, in steps tied to block count. They come from the 21M PAC treasury, which at these rates lasts until around block 44M. #PAC @PactusChain

3b. (tech) Inside #Pactus #3: Pactus block reward (PIP-55), as coded: 1 PAC per block up to height 8M, 0.5 up to 24M, then 0.25. It depends on block height, not calendar time. Rewards come from the 21M PAC treasury, which at these rates runs out around block 44M. #PAC @PactusChain

4a. (simple) Inside #Pactus #4: The first reward halving comes at block 8 million: about 2.5 years after launch in theory, later in practice, because blocks missed during slowdowns are not made up. #PAC @PactusChain

4b. (tech) Inside #Pactus #4: First reward halving is at block 8,000,000: about 2.5 years after genesis if every 10 s slot got a block, the second (24M) about 7.6 years. Missed slots are not made up (a late block takes the current time), so real dates come later. #PAC @PactusChain

5a. (simple) Inside #Pactus #5: Each block sends a small share of its reward to the foundation, rotating across 100 addresses. That share shrinks along with the reward. #PAC @PactusChain

5b. (tech) Inside #Pactus #5: Every Pactus block pays a slice of its reward to a foundation address chosen as `height mod 100` from a list of 100 addresses. The base amount is 0.3 PAC and it halves with the block reward. #PAC @PactusChain

6a. (simple) Inside #Pactus #6: Every block opens with one special transaction that pays the block reward from the treasury. Validators check that the amounts follow the rules before accepting the block. #PAC @PactusChain

6b. (tech) Inside #Pactus #6: Every Pactus block starts with exactly one subsidy transaction: a zero-fee batch transfer from the treasury. Validators check it: the total must equal reward + fees, and the foundation payout and any delegation share must match the rules. #PAC @PactusChain

7a. (simple) Inside #Pactus #7: Block times can't be faked: a block must come after the previous one and close to its expected slot. If the chain fell behind, the next block simply uses the current time. #PAC @PactusChain

7b. (tech) Inside #Pactus #7: Timestamps aren't free-form: seconds must be a multiple of 10, and the time must follow the previous block and be at most one interval past the expected slot. If the chain is >10 s behind, the proposer uses current time, rounded down. #PAC @PactusChain

8a. (simple) Inside #Pactus #8: Pactus mainnet launched on January 24, 2024. Since then: a block every 10 seconds, secured by a committee of 51 validators at a time. #PAC @PactusChain

8b. (tech) Inside #Pactus #8: Pactus mainnet genesis time: 2024-01-24 20:24 UTC (testnet: 2024-03-16 00:00 UTC). Both networks use 10-second blocks and a committee of 51 validators. #PAC @PactusChain

9a. (simple) Inside #Pactus #9: How does a node know which network it's on? It fingerprints the genesis file, the chain's birth certificate, and compares it with the known mainnet and testnet fingerprints. #PAC @PactusChain

9b. (tech) Inside #Pactus #9: The genesis file has no chain ID field. A node hashes the genesis document and compares it to the built-in mainnet and testnet hashes; anything else is a localnet. That picks the address prefix and the p2p network name. #PAC @PactusChain

## Series 2 — Block & certificate format

10a. (simple) Inside #Pactus #10: Every block starts with a small header of exactly 138 bytes: time, who made it, and fingerprints of the previous block and of the whole chain state. Small and predictable. #PAC @PactusChain

10b. (tech) Inside #Pactus #10: A block header is exactly 138 bytes: version (1), time (4), previous hash (32), state root (32), sortition seed (48), proposer address (21). Fixed layout, so a stored block's time is read at a known offset without decoding it. #PAC @PactusChain

11a. (simple) Inside #Pactus #11: A block's ID covers its header, its parent's proof and a fingerprint of all its transactions. Change a single transaction and the block ID changes. #PAC @PactusChain

11b. (tech) Inside #Pactus #11: A Pactus block hash covers the header, the previous certificate's hash, the transaction Merkle root and the tx count. Transactions enter the hash only through their root. #PAC @PactusChain

12a. (simple) Inside #Pactus #12: Transactions are folded into one fingerprint (a Merkle root). Pactus also adds their count to the block ID, closing a known corner case of this kind of tree. #PAC @PactusChain

12b. (tech) Inside #Pactus #12: Pactus's tx Merkle tree hashes an odd node with itself, so the root alone doesn't commit to the leaf count. The block hash adds the count separately to close that gap. #PAC @PactusChain

13a. (simple) Inside #Pactus #13: A Pactus block holds at most 1,000 transactions, and nodes reject anything bigger while reading it, before even validating it. #PAC @PactusChain

13b. (tech) Inside #Pactus #13: A Pactus block holds at most 1,000 transactions. The decoder rejects a length prefix above that before it allocates the transaction list, so the limit holds at parse time, not only during validation. #PAC @PactusChain

14a. (simple) Inside #Pactus #14: Every block is approved by the committee, but the proof stays tiny: a list of who took part and one combined signature instead of dozens. #PAC @PactusChain

14b. (tech) Inside #Pactus #14: A Pactus block certificate = height, round, committers (validator numbers), absentees and ONE aggregated BLS signature. No per-validator signatures on chain; the signers are "committers minus absentees". #PAC @PactusChain

15a. (simple) Inside #Pactus #15: Checking that a block was approved takes a single signature check, however many validators signed. That keeps nodes fast and light. #PAC @PactusChain

15b. (tech) Inside #Pactus #15: Verifying a Pactus certificate takes one BLS check: sum the power of the signers, require 2f+1, aggregate their public keys, verify the single signature. No loop over individual signatures. #PAC @PactusChain

16a. (simple) Inside #Pactus #16: Each block carries the proof that its parent was approved, so the chain links itself together, block after block, back to the first one. #PAC @PactusChain

16b. (tech) Inside #Pactus #16: Each Pactus block embeds the aggregated certificate that finalised its parent, checked against the previous height's committee. Only the genesis block has none. #PAC @PactusChain

17a. (simple) Inside #Pactus #17: Validators are referred to by a short number instead of a full address, so block proofs stay small even with a full committee. #PAC @PactusChain

17b. (tech) Inside #Pactus #17: Certificates list validators by a compact number (int32, varint), not their 21-byte address. Numbers are sequential: genesis validators first, then each new validator when its first bond creates it. Small index, small certificate. #PAC @PactusChain

18a. (simple) Inside #Pactus #18: Each kind of consensus vote signs a different message, so a vote cast for one step can never be reused for another. #PAC @PactusChain

18b. (tech) Inside #Pactus #18: Every consensus vote signs block hash ‖ height ‖ round plus a type tag (PREPARE, PRE-VOTE, MAIN-VOTE, DECIDED; PRECOMMIT has none), and change-proposer votes add the CP round and value. No vote type can pass as another. #PAC @PactusChain

19a. (simple) Inside #Pactus #19: Amounts and fees in a transaction have exactly one valid encoding, so every node reads, hashes and signs the very same bytes. #PAC @PactusChain

19b. (tech) Inside #Pactus #19: Pactus votes are CBOR maps with fixed integer keys (1-7). Transaction amounts and fees are LEB128 varints, and the decoder rejects non-minimal varints, so each integer has exactly one byte encoding in a transaction. #PAC @PactusChain

20a. (simple) Inside #Pactus #20: A transaction's ID depends only on what it says (amount, fee, memo…), not on its signature. #PAC @PactusChain

20b. (tech) Inside #Pactus #20: A Pactus transaction ID is the hash of its signing bytes (version, lock time, fee, memo, payload) and excludes the signature. The ID depends only on what the transaction says. #PAC @PactusChain

## Series 3 — Committee & sortition

21a. (simple) Inside #Pactus #21: Pactus doesn't elect its validators by vote. After each block, every validator runs a private lottery, and the winners can claim a seat on the committee. #PAC @PactusChain

21b. (tech) Inside #Pactus #21: Pactus doesn't vote its committee in. After every block each validator runs a private lottery with the chain seed. Winners broadcast a sortition tx to take a seat. Default committee size: 51. #PAC @PactusChain

22a. (simple) Inside #Pactus #22: The committee lottery can't be rigged: each validator's ticket comes from its own key and the chain's randomness, and anyone can check it. One draw per validator per block. #PAC @PactusChain

22b. (tech) Inside #Pactus #22: Pactus sortition proof = a BLS signature over (seed ‖ public key), hashed with BLAKE2b-256 and scaled into [0, total power). One seed, one deterministic output per key. #PAC @PactusChain

23a. (simple) Inside #Pactus #23: More stake means better odds, but fairly: a validator with 10% of the stake wins about 10% of the lottery draws. #PAC @PactusChain

23b. (tech) Inside #Pactus #23: In Pactus a validator wins sortition when its index is below its own power, with the range set to total network power. So a validator with 10% of the stake wins about 10% of the draws. #PAC @PactusChain

24a. (simple) Inside #Pactus #24: Where does the randomness come from? Each block's proposer signs the previous random value to make the next one, and every node checks it. No one gets to choose it. #PAC @PactusChain

24b. (tech) Inside #Pactus #24: Pactus randomness is a chain of BLS signatures: each block's sortition seed is the proposer's signature over the hash of the previous seed, and every node verifies it against the proposer's key before accepting the block. #PAC @PactusChain

25a. (simple) Inside #Pactus #25: A lottery win must be claimed fast: the ticket expires after 17 blocks, about 3 minutes. Normal transactions stay valid for about a day. #PAC @PactusChain

25b. (tech) Inside #Pactus #25: A Pactus sortition tx is only valid for a short window: 17 blocks (≈3 minutes). Other transactions get 8,640 blocks (≈1 day). #PAC @PactusChain

26a. (simple) Inside #Pactus #26: New validators wait about one hour after staking before they can enter the committee lottery. #PAC @PactusChain

26b. (tech) Inside #Pactus #26: New Pactus validators wait 360 blocks (≈1 hour) after bonding before their sortition txs are accepted. #PAC @PactusChain

27a. (simple) Inside #Pactus #27: The committee changes gradually: at each step, less than a third of its power can be replaced, and the members who leave are the longest-seated ones. #PAC @PactusChain

27b. (tech) Inside #Pactus #27: Pactus bounds committee churn: when the committee is full, the power joining in one step must stay under one third of committee power, and the members who leave are the ones with the oldest sortition. #PAC @PactusChain

28a. (simple) Inside #Pactus #28: While a validator sits on the committee, its stake is frozen: it can't add to it or unstake until it leaves. #PAC @PactusChain

28b. (tech) Inside #Pactus #28: In Pactus your stake is frozen while you sit on the committee: bond and unbond txs for a member are rejected in block validation until the validator leaves. #PAC @PactusChain

29a. (simple) Inside #Pactus #29: Validators take turns proposing blocks, round-robin. A newcomer joins at the back of the line. #PAC @PactusChain

29b. (tech) Inside #Pactus #29: Proposer order is a round-robin walk over a linked list of committee members. New members are inserted just behind the current proposer, so they wait at the back of the queue until the rest of the committee has had a turn. #PAC @PactusChain

30a. (simple) Inside #Pactus #30: A node that restarts doesn't need a saved copy of the committee: it rebuilds it from the latest blocks. #PAC @PactusChain

30b. (tech) Inside #Pactus #30: Pactus restarts without a committee snapshot. The node rebuilds the committee from the last certificate and the sortition txs of the last block, then replays the rotation. #PAC @PactusChain

## Series 4 — Staking

31a. (simple) Inside #Pactus #31: A validator can stake up to 1,000 PAC. Each stake transaction must be at least 1 PAC, unless it tops the validator up to exactly 1,000. #PAC @PactusChain

31b. (tech) Inside #Pactus #31: Pactus stake limits (defaults, same on mainnet): each bond must be at least 1 PAC and a validator's total stake can't exceed 1,000 PAC. A bond under 1 PAC is only accepted if it tops the validator up to exactly 1,000 PAC. #PAC @PactusChain

32a. (simple) Inside #Pactus #32: In Pactus, voting weight = stake. Unstake and your validator's weight drops to zero right away. #PAC @PactusChain

32b. (tech) Inside #Pactus #32: In Pactus, voting power = stake in nanoPAC (1 PAC = 10⁹), except genesis bootstrap validators, which have no stake and power 1. A validator that unbonds drops to zero power in the same block the unbond is executed. #PAC @PactusChain

33a. (simple) Inside #Pactus #33: Unstaking takes 21 days: voting weight goes away at once, and the funds can be withdrawn three weeks later. #PAC @PactusChain

33b. (tech) Inside #Pactus #33: Unbonding a Pactus validator removes its power immediately. The stake can be withdrawn after 181,440 blocks, i.e. 21 days. #PAC @PactusChain

34a. (simple) Inside #Pactus #34: Unstaking and claiming a committee seat cost no fee at all. That's a protocol rule. #PAC @PactusChain

34b. (tech) Inside #Pactus #34: Pactus unbond and sortition transactions carry a fee of exactly zero. It's a protocol rule enforced in basic validation, not only a mempool policy. #PAC @PactusChain

35a. (simple) Inside #Pactus #35: Delegation (PIP-49): a stake owner can let an operator run the validator. The owner gets a capped share of the rewards, and only the owner can unstake, once the agreed period ends. #PAC @PactusChain

35b. (tech) Inside #Pactus #35: PIP-49 delegation: a delegated bond is exactly the max stake (1,000 PAC) with a future expiry. Owner reward share is capped at 0.7 PAC/block (before halving). The owner, not the operator, signs unbond and withdraw; unbond only after expiry. #PAC @PactusChain

36a. (simple) Inside #Pactus #36: How does Pactus upgrade? Validators signal by running the new version. Once more than 75% of committee stake supports it, blocks switch to the new version. #PAC @PactusChain

36b. (tech) Inside #Pactus #36: Protocol upgrades (PIP-51) are signalled by stake: a proposer moves to the new block version only once >75% of committee power runs a node that supports it. No scheduled activation height (the PIP-55 halving is height-based). #PAC @PactusChain

37a. (simple) Inside #Pactus #37: Pactus upgrades so far: split rewards, delegation, reward halving and on-chain anchoring, each one a numbered protocol version. #PAC @PactusChain

37b. (tech) Inside #Pactus #37: Pactus protocol versions so far: v2 split reward (PIP-43), v3 delegation (PIP-49), v4 halving (PIP-55), v5 native state anchoring (PIP-50). One enum value per upgrade. #PAC @PactusChain

## Series 5 — Consensus ⚠️

38a. (simple) Inside #Pactus #38: Pactus consensus counts stake, not heads: a block needs approval from validators holding a supermajority, about two thirds, of committee stake. #PAC @PactusChain

38b. (tech) Inside #Pactus #38: Pactus counts BFT quorums in voting power (stake), not head count. With n = total power, f = (n-1)/3. The v2 consensus code checks f+1, 2f+1 and 3f+1 of it; block certificates need 2f+1. #PAC @PactusChain

39a. (simple) Inside #Pactus #39: The next version of Pactus consensus (v2 code) removes one voting step, so a block gets approved with one round of votes fewer. #PAC @PactusChain

39b. (tech) Inside #Pactus #39: Pactus's v2 consensus code (consensusv2) drops the prepare phase: a block goes straight from proposal to precommit. #PAC @PactusChain

40a. (simple) Inside #Pactus #40: If a block proposer is slow, validators vote to replace it. They wait 5 seconds, then 10, then 15 in later rounds, or start sooner if enough of them already agree. #PAC @PactusChain

40b. (tech) Inside #Pactus #40: Pactus's change-proposer timeout grows linearly: 5 s + 5 s per round (round 0: 5 s, round 1: 10 s, round 2: 15 s), counted from the start of the round. A validator can start earlier if it already sees f+1 change-proposer pre-votes. #PAC @PactusChain

41a. (simple) Inside #Pactus #41: Replacing a slow proposer is a vote of its own, in up to three steps. Each vote must carry a proof of why it was cast, which the other validators check. #PAC @PactusChain

41b. (tech) Inside #Pactus #41: The change-proposer phase has three vote types: pre-vote, main-vote, decided, each with a justification others check. Values are Yes (change proposer) or No (keep it); only main-votes can be Abstain. It repeats in CP rounds until decided. #PAC @PactusChain

42a. (simple) Inside #Pactus #42: In the v2 code, if almost the whole committee (by stake) approves a block, it is committed right away, with no extra voting round. #PAC @PactusChain

42b. (tech) Inside #Pactus #42: v2 fast path: once a validator holds the proposal and sees precommits for it from 3f+1 of total voting power, it commits with no change-proposer round. With only 2f+1 precommits, the change-proposer phase must decide "No" first. #PAC @PactusChain

43a. (simple) Inside #Pactus #43: Validators that are often offline lose their turn: if a proposer's availability score is below two thirds, the committee moves straight to replacing it. #PAC @PactusChain

43b. (tech) Inside #Pactus #43: In Pactus, if the scheduled proposer's availability score is below 0.666667 (PIP-19), validators skip the proposal and go straight to changing the proposer for that round. #PAC @PactusChain

44a. (simple) Inside #Pactus #44: Each validator has an availability score: how often it showed up while on the committee, over roughly the last week. Computed from the chain, not self-reported. #PAC @PactusChain

44b. (tech) Inside #Pactus #44: A validator's availability score = 1 − (certificates it was absent from ÷ certificates it was in the committee for), over the last 60,000 certificates (≈6.9 days at 10 s). Derived from on-chain data, not self-reported. #PAC @PactusChain

45a. (simple) Inside #Pactus #45: Consensus timers are tied to a specific block and round. An old timer that fires late is simply ignored. #PAC @PactusChain

45b. (tech) Inside #Pactus #45: Every timer in Pactus's v2 consensus is tagged with its height and round. A timer that fires after the node has moved on is simply dropped. #PAC @PactusChain

46a. (simple) Inside #Pactus #46: Consensus follows the clock: a new block every 10 seconds. In the v2 code, a node whose clock runs a bit late catches up when it sees the others already voting. #PAC @PactusChain

46b. (tech) Inside #Pactus #46: Consensus is paced on wall-clock time: the next height starts at last block time + the block interval (10 s by default). The v2 code skips the rest of the wait if it already sees 2f+1 precommit power for round 0, for nodes whose clock lags. #PAC @PactusChain

47a. (simple) Inside #Pactus #47: One node can run several validators. They talk to each other inside the node instead of over the network. #PAC @PactusChain

47b. (tech) Inside #Pactus #47: A Pactus node running several validator keys uses an in-process mediator: each proposal and vote is forwarded to its other local instances, and a block announcement moves them all to the next height. #PAC @PactusChain

## Series 6 — Crypto & keys

48a. (simple) Inside #Pactus #48: Pactus validators sign with BLS, a scheme where many signatures can be merged into one. That's why block proofs stay so small. #PAC @PactusChain

48b. (tech) Inside #Pactus #48: Pactus validators sign with BLS12-381: 48-byte signatures (G1), 96-byte public keys (G2), hash-to-curve per the IRTF BLS draft with SHA-256. Signatures add up, which is what makes one-signature certificates possible. #PAC @PactusChain

49a. (simple) Inside #Pactus #49: Pactus accounts support three signature types: BLS, Ed25519 and secp256k1 (Bitcoin's curve). The address tells which one is used. #PAC @PactusChain

49b. (tech) Inside #Pactus #49: Pactus accounts can use three signature schemes: BLS12-381, Ed25519 and secp256k1. The signer's address type decides which one a transaction uses. #PAC @PactusChain

50a. (simple) Inside #Pactus #50: Pactus addresses start with "pc1" on mainnet and "tpc1" on testnet, in bech32m, the same format Bitcoin uses for its newest addresses. #PAC @PactusChain

50b. (tech) Inside #Pactus #50: Pactus addresses = RIPEMD-160(BLAKE2b-256(pubkey)) plus a 1-byte type, encoded as bech32m. HRP "pc" on mainnet, "tpc" on testnet. #PAC @PactusChain

51a. (simple) Inside #Pactus #51: You can tell an address type at a glance: pc1p… is a validator, pc1z… a BLS account, pc1r… Ed25519, pc1y… secp256k1. #PAC @PactusChain

51b. (tech) Inside #Pactus #51: The first character after "pc1" tells the account type in Pactus: pc1p… is a validator, pc1z… a BLS account, pc1r… Ed25519, pc1y… secp256k1. #PAC @PactusChain

52a. (simple) Inside #Pactus #52: The treasury has a special address: all zeros. It can't sign transactions, so its funds only move through protocol rules, like block rewards. #PAC @PactusChain

52b. (tech) Inside #Pactus #52: The Pactus treasury address isn't a bech32m string at all: it's the all-zero address, special-cased in encoding and decoding. #PAC @PactusChain

53a. (simple) Inside #Pactus #53: Pactus wallets are HD wallets: one seed phrase recovers all your keys, organised by key type, with separate coin numbers for mainnet and testnet. #PAC @PactusChain

53b. (tech) Inside #Pactus #53: Pactus HD paths: m/purpose'/coin'/type'/index. Purpose 12381 for BLS, 44 (BIP-44) for Ed25519 and secp256k1, 65535 for imported keys. Coin 21888 mainnet, 21777 testnet. Type = address type (1-4). Index is hardened except for BLS. #PAC @PactusChain

54a. (simple) Inside #Pactus #54: A watch-only Pactus wallet, with no private keys at all, can still create new BLS addresses to receive funds. #PAC @PactusChain

54b. (tech) Inside #Pactus #54: BLS HD keys (PIP-11): HMAC-SHA512 keyed with "BLS12381 seed"; the left half goes through BLS KeyGen, the right half is the chain code. Non-hardened children derive from the public key, so a watch-only wallet can create BLS addresses. #PAC @PactusChain

55a. (simple) Inside #Pactus #55: Wallets are encrypted with Argon2id, a scheme built to resist password guessing. You can also make a watch-only copy that holds no private keys. #PAC @PactusChain

55b. (tech) Inside #Pactus #55: Wallets encrypt with Argon2id (3 iterations, 64 MiB, 4 lanes), then AES-256-CTR with a 4-byte MAC. New wallets use SQLite (old JSON ones can be migrated), and `pactus-wallet neuter` makes a watch-only copy with no private keys. #PAC @PactusChain

56a. (simple) Inside #Pactus #56: With PIP-50, an account can anchor data on chain: a fingerprint of the data plus a link to it, backed by a deposit of at least 1 PAC. Live once the network upgrades to protocol v5. #PAC @PactusChain

56b. (tech) Inside #Pactus #56: With PIP-50 (protocol v5), a Pactus account can carry an anchor: a 32–64 byte root hash plus a URI of up to 128 bytes. Creating one locks a deposit of at least 1 PAC. Native on-chain state anchoring, live once >75% of committee power runs v5. #PAC @PactusChain

## Series 7 — Transactions & mempool

57a. (simple) Inside #Pactus #57: Pactus has 7 built-in transaction types: transfer, stake, unstake, withdraw, committee claim, batch transfer and anchor. Each one is handled natively by the protocol. #PAC @PactusChain

57b. (tech) Inside #Pactus #57: Pactus has 7 transaction types built into the protocol: transfer, bond, unbond, withdraw, sortition, batch transfer and anchor. Each has its own executor. Anchor only becomes valid once blocks reach protocol version 5 (PIP-50). #PAC @PactusChain

58a. (simple) Inside #Pactus #58: Fees are simple: a fixed 0.01 PAC by default, and unstaking is free. No bidding war to get your transaction in. #PAC @PactusChain

58b. (tech) Inside #Pactus #58: Fees: the default minimum is a fixed 0.01 PAC for transfers, bonds, withdrawals, batch transfers and anchors; unbond and sortition are free. It's node mempool policy, not a consensus rule. Mempool is FIFO per type: no fee auction. #PAC @PactusChain

59a. (simple) Inside #Pactus #59: An optional extra fee can kick in for accounts that send a lot of data in a day. It's off by default. #PAC @PactusChain

59b. (tech) Inside #Pactus #59: Pactus has an optional consumption-based fee on top of the fixed one, driven by the bytes an account sent over the last 8,640 blocks. Past the 360-byte daily limit it grows faster than linearly. Unit price defaults to 0, so it's off. #PAC @PactusChain

60a. (simple) Inside #Pactus #60: A transaction stays valid for about one day. Not included by then? It expires. Memos can be up to 64 bytes. #PAC @PactusChain

60b. (tech) Inside #Pactus #60: In Pactus a transaction's lock time is also its expiry: it stays valid for 8,640 blocks (about 1 day at 10 s blocks; sortition txs get only 17). Memos are capped at 64 bytes. #PAC @PactusChain

61a. (simple) Inside #Pactus #61: A batch transfer pays 2 to 8 people in one transaction, for a single fee. #PAC @PactusChain

61b. (tech) Inside #Pactus #61: A Pactus batch transfer pays 2 to 8 distinct recipients in one signed transaction. The sender pays the total plus a single fee. #PAC @PactusChain

62a. (simple) Inside #Pactus #62: The waiting room for transactions (the mempool) has a separate lane per type, so a flood of transfers can't crowd out staking or other transactions. #PAC @PactusChain

62b. (tech) Inside #Pactus #62: Pactus's mempool has one bounded sub-pool per transaction type. With the default max_size of 1,000, transfers get 500 slots and each of the other six types 100, so the pool holds up to 1,100 txs. A full sub-pool drops its oldest entry. #PAC @PactusChain

63a. (simple) Inside #Pactus #63: Block proposers pick waiting transactions in a fixed order by type, then first come, first served. No fee auction. #PAC @PactusChain

63b. (tech) Inside #Pactus #63: Pactus's proposer takes mempool txs in a fixed type order: sortition, bond, unbond, withdraw, transfer, batch, anchor; first in, first out within a type. It re-checks them, caps the block at 1,000 txs and puts the subsidy tx first. #PAC @PactusChain


## Series 8 — State & storage

64a. (simple) Inside #Pactus #64: The whole chain state, all accounts and validators, is summed up in one fingerprint stored in every block. Updating it only reworks the parts that changed. #PAC @PactusChain

64b. (tech) Inside #Pactus #64: Pactus's state root combines two incremental Merkle trees, one for accounts and one for validators, hashed with BLAKE2b-256. A block only rehashes the paths of the leaves it changed. #PAC @PactusChain

65a. (simple) Inside #Pactus #65: Every account and validator gets a sequential number when it's created, which is also its place in that state fingerprint. #PAC @PactusChain

65b. (tech) Inside #Pactus #65: Every Pactus account and validator gets a sequential number at creation. That number is also its leaf position in the Merkle state tree. #PAC @PactusChain

66a. (simple) Inside #Pactus #66: Each block is saved in one atomic write: if a node crashes mid-save, it never ends up half-updated. #PAC @PactusChain

66b. (tech) Inside #Pactus #66: Pactus commits each block in a single LevelDB batch: block bytes, tx index, account and validator updates, public keys, last certificate. A crash can't leave them out of sync. #PAC @PactusChain

67a. (simple) Inside #Pactus #67: Looking up a transaction is quick: the node knows exactly where it sits inside its block. #PAC @PactusChain

67b. (tech) Inside #Pactus #67: Pactus's tx index stores (height, offset, length) per transaction ID. Reading a transaction slices the stored block bytes directly, without decoding the whole block. #PAC @PactusChain

68a. (simple) Inside #Pactus #68: Public keys are stored once per account instead of in every transaction, which saves disk space. #PAC @PactusChain

68b. (tech) Inside #Pactus #68: Each signer's public key lives in a separate index. After a signer's first appearance, its txs are stored without the public key in the block bytes, and the key is restored from the index when you read the block or tx. #PAC @PactusChain

69a. (simple) Inside #Pactus #69: Short on disk space? A node can be pruned to keep only the last 30 days of blocks (10 days minimum). Nodes keep full history unless you choose to prune. #PAC @PactusChain

69b. (tech) Inside #Pactus #69: Prune mode: default retention is 30 days of blocks (259,200 at 10 s), minimum 10 days. `pactus-daemon prune` converts an existing full node to prune mode. A node syncing from genesis keeps full history unless you prune it. #PAC @PactusChain

## Series 9 — Networking & sync

70a. (simple) Inside #Pactus #70: Catching up is pull-based: a node that falls behind asks peers for the blocks it misses. Gossip is only for fresh news: new blocks, transactions and votes. #PAC @PactusChain

70b. (tech) Inside #Pactus #70: Pactus sync is pull-based by design: a node that falls behind asks peers for blocks over direct streams instead of being sent them. Gossip is for fresh data only: new-block announcements, transactions and consensus messages. #PAC @PactusChain

71a. (simple) Inside #Pactus #71: Pactus nodes gossip on three channels: blocks, transactions and consensus. Each network has its own channel names, so mainnet and testnet never mix. #PAC @PactusChain

71b. (tech) Inside #Pactus #71: Pactus gossip runs on three libp2p gossipsub topics: block, transaction, consensus. Topic names carry the network name, so networks stay separated at the libp2p level. #PAC @PactusChain

72a. (simple) Inside #Pactus #72: All Pactus network messages share one envelope format, and each message type knows whether it's broadcast or sent to a single peer. #PAC @PactusChain

72b. (tech) Inside #Pactus #72: Every Pactus P2P message travels in the same CBOR envelope: flags, message type, payload, consensus height. Each message type declares whether it's gossiped or sent point to point. #PAC @PactusChain

73a. (simple) Inside #Pactus #73: A node catching up downloads from several peers at once: up to 8 parallel batches of 720 blocks. #PAC @PactusChain

73b. (tech) Inside #Pactus #73: A Pactus node catching up opens up to 8 parallel download sessions of 720 blocks each; the serving peer answers in 60-block messages. #PAC @PactusChain

74a. (simple) Inside #Pactus #74: Blocks may arrive out of order, but they are always added to the chain in order, each with its proof. #PAC @PactusChain

74b. (tech) Inside #Pactus #74: Pactus syncs in parallel but commits in strict order: downloaded blocks wait in an LRU cache keyed by height, and are committed only when the next height and its certificate are present. #PAC @PactusChain

75a. (simple) Inside #Pactus #75: For downloads, nodes favour peers that delivered reliably, while still giving the others a chance. #PAC @PactusChain

75b. (tech) Inside #Pactus #75: Block-download peers are picked at random, weighted by track record: score = (completed+1)/(total+1) sessions, never below 1%. New peers start at the top; failing peers rarely get picked but stay selectable. Only handshaken peers qualify. #PAC @PactusChain

76a. (simple) Inside #Pactus #76: Very far behind, about 10 days or more? A node then downloads only from peers that keep the full history. #PAC @PactusChain

76b. (tech) Inside #Pactus #76: Pactus nodes advertise a FULL flag if they keep the whole chain history. A node more than 86,400 blocks (about 10 days) behind downloads only from FULL peers; within that window any handshaken peer, pruned or full, can serve it. #PAC @PactusChain

77a. (simple) Inside #Pactus #77: Before exchanging blocks, two nodes say hello and acknowledge each other. Only peers that complete this handshake are used for syncing. #PAC @PactusChain

77b. (tech) Inside #Pactus #77: Handshake: the outbound peer sends Hello; the inbound peer replies with a HelloAck and its own Hello; the outbound peer acks it. A peer is "known" only after both Hellos are accepted, and only known peers request or serve blocks. #PAC @PactusChain

78a. (simple) Inside #Pactus #78: Behind a home router? Pactus nodes detect it and use relays and hole punching to stay reachable. #PAC @PactusChain

78b. (tech) Inside #Pactus #78: Pactus nodes behind NAT use libp2p AutoNAT v2 to detect reachability, then fall back to relays with hole punching. UPnP is opt-in. #PAC @PactusChain

79a. (simple) Inside #Pactus #79: Nodes cap their connections (64 by default, fewer outgoing) and reconnect automatically if they drop below a minimum. #PAC @PactusChain

79b. (tech) Inside #Pactus #79: Pactus caps connections by direction: inbound up to 64 by default (max_connections), outbound a quarter of that (16). Every 60s, if fewer than 14 peers (64/4 - 2) are connected, it redials its saved and bootstrap peers in the background. #PAC @PactusChain

80a. (simple) Inside #Pactus #80: A node remembers its peers when it shuts down cleanly and reconnects to them at the next start. The mainnet bootstrap list is built in. #PAC @PactusChain

80b. (tech) Inside #Pactus #80: A Pactus node saves the peers it has connected to, plus bootstrap entries, to peers.json on clean shutdown and reloads them next to the bootstrap list at startup. Mainnet's bootstrap list is embedded in the binary. #PAC @PactusChain

## Series 10 — Node, API & tooling

81a. (simple) Inside #Pactus #81: The node's API is defined once, and gRPC, REST, JSON-RPC and the docs are all generated from that single definition. One source, always in sync. #PAC @PactusChain

81b. (tech) Inside #Pactus #81: The Pactus node API is defined once in `.proto` files. One `buf generate` emits gRPC stubs, a REST gateway, JSON-RPC bindings, OpenAPI, OpenRPC and docs. #PAC @PactusChain

82a. (simple) Inside #Pactus #82: Out of the box, a mainnet node only opens its main API to the local machine. Everything else is opt-in. #PAC @PactusChain

82b. (tech) Inside #Pactus #82: Pactus's node gRPC API: 5 services, 54 RPCs. The wallet service is opt-in (enable_wallet=false). With mainnet defaults only gRPC is on, bound to 127.0.0.1:50051; REST, JSON-RPC and ZeroMQ are off. (Testnet defaults differ.) #PAC @PactusChain

83a. (simple) Inside #Pactus #83: Developers also get JSON-RPC and REST, with an interactive Swagger page built into the node. #PAC @PactusChain

83b. (tech) Inside #Pactus #83: Pactus's JSON-RPC server is a thin layer over the node's own gRPC server, and the REST gateway ships with a Swagger UI embedded in the binary via `go:embed`. #PAC @PactusChain

84a. (simple) Inside #Pactus #84: Apps can subscribe to a live feed of new blocks and transactions over ZeroMQ. Off by default. #PAC @PactusChain

84b. (tech) Inside #Pactus #84: Nodes can publish committed blocks and their txs on ZeroMQ: 5 feeds (block info, tx info, raw block, raw tx, anchor info), each tagged with a 2-byte topic ID. Off by default. Pure-Go zmq4, no libzmq. Raw block = header only. #PAC @PactusChain

85a. (simple) Inside #Pactus #85: pactus-shell lets you query a node from the command line, with an interactive mode. #PAC @PactusChain

85b. (tech) Inside #Pactus #85: `pactus-shell` is a gRPC CLI with an interactive prompt mode. Its commands are generated from the same `.proto` services as the gRPC API, so it follows the API once the code is regenerated and rebuilt. #PAC @PactusChain

86a. (simple) Inside #Pactus #86: The wallet can talk to your own node, to public servers, or work fully offline. #PAC @PactusChain

86b. (tech) Inside #Pactus #86: The Pactus wallet talks to the chain through a provider: a local node, a remote gRPC server list, or offline mode with no network calls at all. #PAC @PactusChain

87a. (simple) Inside #Pactus #87: Two nodes can't accidentally run on the same data folder: the second one refuses to start. #PAC @PactusChain

87b. (tech) Inside #Pactus #87: `pactus-daemon` takes an exclusive file lock on its working directory before starting or pruning. A second instance on the same data prints a warning and exits. #PAC @PactusChain

88a. (simple) Inside #Pactus #88: The official Docker image is small: static binaries on plain Alpine Linux. #PAC @PactusChain

88b. (tech) Inside #Pactus #88: The Pactus Docker image is a two-stage build: static binaries (CGO off, `-trimpath`, stripped) copied into plain Alpine. #PAC @PactusChain

89a. (simple) Inside #Pactus #89: Pactus tests are reproducible: random test data comes from a logged seed, and integration tests spin up a small 4-node network. #PAC @PactusChain

89b. (tech) Inside #Pactus #89: Test helpers draw random values (keys, heights, amounts, blocks) from a seed each test logs. To reproduce a failure, build the suite with NewTestSuiteFromSeed and that seed. Integration tests boot four in-process nodes with fixed keys. #PAC @PactusChain

90a. (simple) Inside #Pactus #90: Version checks ignore tags like "beta": 1.18.0-beta counts as 1.18.0. #PAC @PactusChain

90b. (tech) Inside #Pactus #90: Pactus compares versions by major.minor.patch only: "1.18.0-beta" ranks the same as 1.18.0. A deliberate departure from SemVer, with the version baked in at build time. #PAC @PactusChain

