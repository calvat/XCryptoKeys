# Thread : The math that guards your coins

- Sujet : 3. La cryptographie des cryptos
- Angle : signatures, hash et seed phrases expliqués par leurs détails, avec les échecs célèbres.
- Préfixe : `#Crypto Math #N:`
- Hashtags fixes : #Cryptography #Blockchain
- Statut : draft (76 posts : 76 checked, 0 draft)

Notes :
- Longueurs comptées comme X (caractères hors latin comptés 2). Toutes les versions font moins de 280.
- Sources : BIPs, EIPs et code source (GitHub) en priorité, puis post-mortems et presse pour les incidents.
- Post 9 : la formule de récupération de clé a aussi été vérifiée par calcul (script Python sur secp256k1).
- Post 75 (BIP360, aussi cité aux posts 72-73) décrit un brouillon qui change souvent de nom : revérifier son statut avant de publier.
- Les montants volés (posts 60, 63) sont en dollars au moment des faits.

---

### 1/ · checked · choix :

**1a (simple)** : #Crypto Math #1: Your coins aren't guarded by a vault. They're guarded by a number. A private key is one pick out of about 10^77 possibilities, far too many for any computer to guess. One fact a day on the math that guards your coins. #Cryptography #Blockchain

**1b (tech)** : #Crypto Math #1: A Bitcoin or Ethereum private key is an integer from 1 to n-1, where n (the secp256k1 group order) is about 1.158 x 10^77, just under 2^256. All security rests on it being truly random. Daily series on the math behind it. #Cryptography #Blockchain

**1c (variante)** : #Crypto Math #1: Not a password, not a file: your crypto is a random number below about 1.158 x 10^77. Seed words, addresses, signatures are all math built on top of it. Let's open the box, one post a day. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 2/ · checked · choix :

**2a (simple)** : #Crypto Math #2: Bitcoin and Ethereum both use the same elliptic curve, called secp256k1. It was almost never used before Bitcoin: most software relied on the NIST curves instead. #Cryptography #Blockchain

**2b (tech)** : #Crypto Math #2: secp256k1 is a Koblitz curve from the SEC 2 standard, not one of the popular NIST curves such as P-256. Bitcoin picked it and Ethereum followed. It was almost never used before Bitcoin. #Cryptography #Blockchain

Source : https://en.bitcoin.it/wiki/Secp256k1

### 3/ · checked · choix :

**3a (simple)** : #Crypto Math #3: The curve behind Bitcoin is a strikingly short formula: y^2 = x^3 + 7. All the math happens on whole numbers that wrap around a huge prime, 78 digits long. #Cryptography #Blockchain

**3b (tech)** : #Crypto Math #3: secp256k1: y^2 = x^3 + 7 over the prime field p = 2^256 - 2^32 - 977 (0xFFFF...FFFEFFFFFC2F). No 'a*x' term, b = 7. That special shape enables fast implementations. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 4/ · checked · choix :

**4a (simple)** : #Crypto Math #4: Why trust secp256k1? Its constants were chosen in a predictable way. That leaves less room for a hidden backdoor than curves whose constants can't be fully explained. #Cryptography #Blockchain

**4b (tech)** : #Crypto Math #4: Unlike the NIST curves, secp256k1's constants come from a predictable, non-random construction. That lowers the room for a planted weakness and allows optimizations often more than 30% faster. #Cryptography #Blockchain

Source : https://en.bitcoin.it/wiki/Secp256k1

### 5/ · checked · choix :

**5a (simple)** : #Crypto Math #5: Your public key is your private key multiplied by a fixed point on the curve. Going forward takes a split second. Going backward, from public key to private key, is believed infeasible. #Cryptography #Blockchain

**5b (tech)** : #Crypto Math #5: Public key P = d*G: scalar multiplication of the generator point G by the private key d. Recovering d from P is the elliptic curve discrete logarithm problem (ECDLP), the hard problem behind every wallet. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 6/ · checked · choix :

**6a (simple)** : #Crypto Math #6: A Bitcoin public key can be written in 65 bytes or just 33. The short form keeps x plus one hint for y, since y can be recomputed from the curve formula. #Cryptography #Blockchain

**6b (tech)** : #Crypto Math #6: Uncompressed pubkey: 0x04 || x || y = 65 bytes. Compressed: 0x02/0x03 (parity of y) || x = 33 bytes. y^2 = x^3 + 7 has two roots, the prefix picks one. Bitcoin Core: SIZE 65, COMPRESSED_SIZE 33. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bitcoin/blob/master/src/pubkey.h

### 7/ · checked · choix :

**7a (simple)** : #Crypto Math #7: A 256-bit key doesn't give 256 bits of security. The best attacks need about the square root of the work, so you get 128 bits. Still far beyond any classical computer. #Cryptography #Blockchain

**7b (tech)** : #Crypto Math #7: secp256k1 targets a 128-bit security level: generic attacks like Pollard's rho solve ECDLP in about sqrt(n), i.e. 2^128 group operations. BIP340 states 128 bits as its target. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 8/ · checked · choix :

**8a (simple)** : #Crypto Math #8: A classic Bitcoin signature is two numbers, r and s, wrapped in a format called DER. Its size varies a little: at most 72 bytes, about 71.5 on average. #Cryptography #Blockchain

**8b (tech)** : #Crypto Math #8: ECDSA signature = (r, s), DER-encoded. With low-S and strict DER, R is at most 33 bytes and S at most 32: total at most 72 bytes, 71.494 on average (BIP62). #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0062.mediawiki

### 9/ · checked · choix :

**9a (simple)** : #Crypto Math #9: Every ECDSA signature needs a fresh secret random number. Use the same one twice and anyone can compute your private key from just two signatures. No hacking, only algebra. #Cryptography #Blockchain

**9b (tech)** : #Crypto Math #9: ECDSA: s = k^-1 (z + r*d) mod n. Two signatures with the same nonce k share r. Then k = (z1 - z2)/(s1 - s2) and d = (s1*k - z1)/r. A reused nonce is a leaked private key. #Cryptography #Blockchain

Source : https://en.wikipedia.org/wiki/Elliptic_Curve_Digital_Signature_Algorithm

### 10/ · checked · choix :

**10a (simple)** : #Crypto Math #10: In 2010, the hacker group fail0verflow showed that Sony signed PS3 software with the same 'random' number every time. That one mistake exposed Sony's private signing key. #Cryptography #Blockchain

**10b (tech)** : #Crypto Math #10: PS3, Chaos Communication Congress 2010: fail0verflow showed Sony's ECDSA code did not generate a different random nonce per signature. Two signatures with the same k were enough to recover Sony's private key. #Cryptography #Blockchain

Source : https://en.wikipedia.org/wiki/PlayStation_3_homebrew

### 11/ · checked · choix :

**11a (simple)** : #Crypto Math #11: August 2013: Android's random number generator had a flaw. Bitcoin wallet apps reused 'random' values in signatures, thieves computed the private keys and took the coins. #Cryptography #Blockchain

**11b (tech)** : #Crypto Math #11: Aug 11, 2013 alert: Android's Java SecureRandom could produce colliding values, so Android wallets emitted ECDSA signatures with repeated r. Private keys were solved from them and funds taken. #Cryptography #Blockchain

Source : https://www.theregister.com/2013/08/12/android_bug_batters_bitcoin_wallets/

### 12/ · checked · choix :

**12a (simple)** : #Crypto Math #12: The cure for bad random numbers in signatures: don't use them. Modern wallets compute the secret number from your key and the message, so it's unique per message and never left to chance. #Cryptography #Blockchain

**12b (tech)** : #Crypto Math #12: RFC 6979: derive the ECDSA nonce deterministically from the private key and message hash (HMAC-DRBG). Same message, same k; different message, different k. libsecp256k1 supports it. #Cryptography #Blockchain

Source : https://github.com/bitcoin-core/secp256k1/blob/master/README.md

### 13/ · checked · choix :

**13a (simple)** : #Crypto Math #13: Odd ECDSA quirk: every signature has a twin. Flip one value and it's still valid. Bitcoin nodes now only relay the 'low' version, so outsiders can't tweak a signature. #Cryptography #Blockchain

**13b (tech)** : #Crypto Math #13: ECDSA malleability: if (r, s) is valid, so is (r, n - s). BIP146 LOW_S requires s <= n/2 (0x7FFF...5D576E7357A4501DDFE92F46681B20A0), closing a third-party way to alter txids. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0146.mediawiki

### 14/ · checked · choix :

**14a (simple)** : #Crypto Math #14: In 2015, an OpenSSL update changed which signatures it accepted, and some Bitcoin nodes rejected the chain. Lesson: consensus code can't rely on a general library's quirks. #Cryptography #Blockchain

**14b (tech)** : #Crypto Math #14: BIP66 (strict DER, created Jan 2015): OpenSSL 1.0.0p and 1.0.1k stopped accepting some non-DER signatures, making some nodes reject the chain. Bitcoin then defined exact DER rules itself. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0066.mediawiki

### 15/ · checked · choix :

**15a (simple)** : #Crypto Math #15: Bitcoin developers wrote their own math library for its curve. When Bitcoin Core switched to it in 2016, checking signatures became up to 7 times faster. #Cryptography #Blockchain

**15b (tech)** : #Crypto Math #15: Bitcoin Core 0.12.0 (Feb 2016) moved signature validation from OpenSSL to libsecp256k1: up to 7x faster on 64-bit, and block validation and reindex took less than half the time. #Cryptography #Blockchain

Source : https://bitcoincore.org/en/2016/02/23/release-0.12.0/

### 16/ · checked · choix :

**16a (simple)** : #Crypto Math #16: Schnorr signatures are simpler than ECDSA, but they were patented. The patent only expired in February 2010, a year after Bitcoin launched with ECDSA. #Cryptography #Blockchain

**16b (tech)** : #Crypto Math #16: Claus Schnorr's scheme (journal paper 1991) was covered by US patent 4,995,082, which expired in Feb 2010. Bitcoin launched in Jan 2009 with ECDSA; Schnorr came only with Taproot (BIP340). #Cryptography #Blockchain

Source : https://en.wikipedia.org/wiki/Schnorr_signature

### 17/ · checked · choix :

**17a (simple)** : #Crypto Math #17: Taproot's Schnorr signatures have a fixed size: always 64 bytes. Public keys shrink too, to 32 bytes. Less data on-chain means lower fees. #Cryptography #Blockchain

**17b (tech)** : #Crypto Math #17: BIP340: fixed 64-byte Schnorr signatures (R.x || s) vs DER ECDSA up to 72 bytes. Public keys are 32-byte x-only (implicit even y) vs 33-byte compressed. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 18/ · checked · choix :

**18a (simple)** : #Crypto Math #18: Schnorr signatures come with a mathematical proof of security under standard assumptions. ECDSA's best known proofs need stronger assumptions. #Cryptography #Blockchain

**18b (tech)** : #Crypto Math #18: BIP340: Schnorr is SUF-CMA secure in the random oracle model assuming ECDLP is hard. The best known provable-security results for ECDSA rely on stronger assumptions. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 19/ · checked · choix :

**19a (simple)** : #Crypto Math #19: Schnorr has a superpower called linearity: keys and signatures can be added. Several people can jointly make one signature valid for the sum of their keys. #Cryptography #Blockchain

**19b (tech)** : #Crypto Math #19: Schnorr linearity: s*G = R + e*P. Summing nonces and partial signatures gives a valid signature for the aggregate key. It's the building block of MuSig, MuSig2 and FROST. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 20/ · checked · choix :

**20a (simple)** : #Crypto Math #20: With MuSig2, a 3-of-3 wallet looks exactly like a normal single-key wallet on the blockchain. Outsiders can't tell that several people signed. #Cryptography #Blockchain

**20b (tech)** : #Crypto Math #20: BIP327 MuSig2: n signers produce one aggregate public key and one 64-byte Schnorr signature. MuSig2 Taproot outputs are indistinguishable from single-signer Taproot outputs. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0327.mediawiki

### 21/ · checked · choix :

**21a (simple)** : #Crypto Math #21: MuSig2 needs only two rounds of messages between signers, and the first round can be done before you even know what you'll sign. #Cryptography #Blockchain

**21b (tech)** : #Crypto Math #21: MuSig2 is a two-round multisig (MuSig1 needed three). Round 1, the nonce exchange, can be preprocessed before the message is known; then partial signatures need no further interaction. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0327.mediawiki

### 22/ · checked · choix :

**22a (simple)** : #Crypto Math #22: Schnorr signatures can be checked in batches: a node can verify many signatures together, faster than checking each one separately. #Cryptography #Blockchain

**22b (tech)** : #Crypto Math #22: BIP340 uses the (R, s) form so s*G = R + e*P has no curve operations inside the hash. That enables batch verification. Standard ECDSA can't be batch-verified faster without extra data. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 23/ · checked · choix :

**23a (simple)** : #Crypto Math #23: Taproot hashes carry a label, like 'BIP0340/challenge', so a hash made for one purpose can never be mistaken for a hash made for another. #Cryptography #Blockchain

**23b (tech)** : #Crypto Math #23: BIP340 tagged hashes: SHA256(SHA256(tag) || SHA256(tag) || x). The 64-byte prefix fills exactly one SHA-256 block, so it can be precomputed. Result: domain separation. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0340.mediawiki

### 24/ · checked · choix :

**24a (simple)** : #Crypto Math #24: A Taproot address can hide a whole set of backup spending rules inside one public key. Spend the normal way, and nobody learns those rules existed. #Cryptography #Blockchain

**24b (tech)** : #Crypto Math #24: BIP341: output key Q = P + hash(P || m)*G, where m is the Merkle root of the script tree. Key-path spends reveal nothing about scripts; script-path spends reveal only the branch used. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0341.mediawiki

### 25/ · checked · choix :

**25a (simple)** : #Crypto Math #25: Taproot stores spending scripts in a tree. To use one, you reveal only that branch and a short proof, not every possible condition. #Cryptography #Blockchain

**25b (tech)** : #Crypto Math #25: Taproot control block = 33 + 32m bytes, Merkle path length m from 0 to 128. The 128 cap: a script with under 1/2^128 chance of use is beyond the security bound anyway. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0341.mediawiki

### 26/ · checked · choix :

**26a (simple)** : #Crypto Math #26: Schnorr signatures went live on Bitcoin with the Taproot upgrade in November 2021. #Cryptography #Blockchain

**26b (tech)** : #Crypto Math #26: Taproot (BIP340/341/342) activated at block 709,632 on Nov 14, 2021, adding Schnorr signatures and SegWit v1 outputs next to ECDSA. #Cryptography #Blockchain

Source : https://www.blockchain.com/explorer/blocks/btc/709632

### 27/ · checked · choix :

**27a (simple)** : #Crypto Math #27: A Bitcoin block's ID is the fingerprint of a tiny 80-byte header, run through SHA-256 twice. Change one bit of the block and the ID changes completely. #Cryptography #Blockchain

**27b (tech)** : #Crypto Math #27: Block hash = SHA256(SHA256(header)). 80-byte header: version (4), prev hash (32), Merkle root (32), time (4), nBits (4), nonce (4). Miners vary the nonce (and more) to hit the target. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bitcoin/blob/master/src/primitives/block.h

### 28/ · checked · choix :

**28a (simple)** : #Crypto Math #28: Bitcoin's very first block has an ID starting with ten zeros. Mining is a hunt for a hash that starts with enough zeros, i.e. a number below a target. #Cryptography #Blockchain

**28b (tech)** : #Crypto Math #28: Genesis block hash: 000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f. Valid proof of work = the hash, read as a 256-bit number, is below the target encoded in nBits. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bitcoin/blob/master/src/kernel/chainparams.cpp

### 29/ · checked · choix :

**29a (simple)** : #Crypto Math #29: By the end of 2015, Bitcoin miners had computed about 2^84 hashes in total. A number that big changed which hash sizes count as safe. #Cryptography #Blockchain

**29b (tech)** : #Crypto Math #29: BIP141: by end of 2015, about 2^84 hashes had been computed in Bitcoin mining, so 2^80 work 'is not infeasible anymore'. That's why P2WSH uses a 32-byte SHA-256 instead of a 20-byte hash. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0141.mediawiki

### 30/ · checked · choix :

**30a (simple)** : #Crypto Math #30: Birthday paradox: finding two inputs with the same 160-bit hash takes about 2^80 tries, not 2^160. That's why SegWit multisig scripts use 256-bit hashes. #Cryptography #Blockchain

**30b (tech)** : #Crypto Math #30: Collision security is half the output size: 160-bit hash, about 2^80. A malicious co-signer could grind a collision on a P2SH multisig. P2WSH (34-byte scriptPubKey vs 23) uses SHA-256 instead. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0141.mediawiki

### 31/ · checked · choix :

**31a (simple)** : #Crypto Math #31: A classic Bitcoin address doesn't contain your public key, only a fingerprint of it, hashed by two different functions in a row. #Cryptography #Blockchain

**31b (tech)** : #Crypto Math #31: P2PKH and P2WPKH commit to HASH160 = RIPEMD-160(SHA-256(pubkey)), a 20-byte hash. Bitcoin Core's CHash160: SHA-256 first, then RIPEMD-160. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bitcoin/blob/master/src/hash.h

### 32/ · checked · choix :

**32a (simple)** : #Crypto Math #32: Old-style Bitcoin addresses never contain 0, O, I or l. They were left out because they're too easy to confuse when reading or copying by hand. #Cryptography #Blockchain

**32b (tech)** : #Crypto Math #32: Base58 alphabet: 123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz. 58 symbols: digits and letters minus 0, O, I and l. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bitcoin/blob/master/src/base58.cpp

### 33/ · checked · choix :

**33a (simple)** : #Crypto Math #33: Legacy addresses end with a 4-byte checksum, a hash of the rest. A typo almost always fails the check, but it can't tell you where the mistake is. #Cryptography #Blockchain

**33b (tech)** : #Crypto Math #33: Base58Check appends the first 4 bytes of SHA256(SHA256(payload)). BIP173 notes this hash checksum is slow and has no error-detection guarantees. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki

### 34/ · checked · choix :

**34a (simple)** : #Crypto Math #34: bc1 addresses (Bech32) use an error-detecting code: any typo touching up to 4 characters is always caught. Guaranteed, not just likely. #Cryptography #Blockchain

**34b (tech)** : #Crypto Math #34: Bech32 (BIP173) uses a BCH code: guaranteed detection of any error affecting at most 4 characters, and less than a 1 in 10^9 chance of missing larger errors. Max length 90. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki

### 35/ · checked · choix :

**35a (simple)** : #Crypto Math #35: Why are bc1 addresses all lowercase? A single case is easier to read aloud, type on a phone and fit into QR codes. Mixed case is rejected. #Cryptography #Blockchain

**35b (tech)** : #Crypto Math #35: BIP173: decoders MUST NOT accept mixed-case Bech32 strings. One case avoids base58's mixed-case pain and allows uppercase for compact alphanumeric QR codes. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki

### 36/ · checked · choix :

**36a (simple)** : #Crypto Math #36: Bech32 had a surprise flaw: if an address ends in 'p', you can add or remove 'q' letters just before it and the checksum still passes. #Cryptography #Blockchain

**36b (tech)** : #Crypto Math #36: BIP350: in Bech32, when the final char is 'p', inserting or deleting any number of 'q' right before it keeps the checksum valid. Fixed-length SegWit v0 addresses weren't affected. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0350.mediawiki

### 37/ · checked · choix :

**37a (simple)** : #Crypto Math #37: The fix is called Bech32m: same design, one constant changed. Taproot addresses (bc1p...) use it; older bc1q addresses keep Bech32. #Cryptography #Blockchain

**37b (tech)** : #Crypto Math #37: Bech32m (BIP350) only changes the final XOR constant, to 0x2bc830a3. SegWit v0 (P2WPKH, P2WSH) stays Bech32; versions 1 to 16, incl. Taproot, use Bech32m. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0350.mediawiki

### 38/ · checked · choix :

**38a (simple)** : #Crypto Math #38: An Ethereum address is the last 20 bytes of a hash of your public key. That's why every address is 40 hex characters after the 0x. #Cryptography #Blockchain

**38b (tech)** : #Crypto Math #38: Ethereum address = last 20 bytes of Keccak-256(pubkey), with pubkey the 64-byte x || y (0x04 prefix dropped). In geth: Keccak256(pubBytes[1:])[12:]. #Cryptography #Blockchain

Source : https://github.com/ethereum/go-ethereum/blob/master/crypto/crypto.go

### 39/ · checked · choix :

**39a (simple)** : #Crypto Math #39: Ethereum's hash is often called 'SHA-3', but it isn't exactly. It's the original Keccak, which gives different outputs from the official SHA3-256. #Cryptography #Blockchain

**39b (tech)** : #Crypto Math #39: Ethereum uses legacy Keccak-256, not FIPS 202 SHA3-256: same permutation, different padding, so outputs differ. geth builds it with NewLegacyKeccak256. #Cryptography #Blockchain

Source : https://github.com/ethereum/go-ethereum/blob/master/crypto/crypto.go

### 40/ · checked · choix :

**40a (simple)** : #Crypto Math #40: Ever noticed random capital letters in Ethereum addresses? They're a checksum. A mistyped address has only about a 0.02% chance of passing it. #Cryptography #Blockchain

**40b (tech)** : #Crypto Math #40: EIP-55: hex letter i is uppercased if bit 4*i of Keccak-256(lowercase hex address) is 1. About 15 check bits per address, so a random typo passes 0.0247% of the time. #Cryptography #Blockchain

Source : https://github.com/ethereum/ercs/blob/master/ERCS/erc-55.md

### 41/ · checked · choix :

**41a (simple)** : #Crypto Math #41: Ethereum transactions don't say who sent them. Nodes compute the sender's public key from the signature itself, then derive the address. #Cryptography #Blockchain

**41b (tech)** : #Crypto Math #41: Ethereum txs carry (v, r, s), no 'from' field. The sender's pubkey is recovered from the ECDSA signature (ecrecover); v encodes the parity of y of the point whose x is r. #Cryptography #Blockchain

Source : https://github.com/ethereum/EIPs/blob/master/EIPS/eip-155.md

### 42/ · checked · choix :

**42a (simple)** : #Crypto Math #42: Since EIP-155, an Ethereum signature includes the chain's ID, so a transaction signed for one network can't be replayed on another. #Cryptography #Blockchain

**42b (tech)** : #Crypto Math #42: EIP-155 (from block 2,675,000): sign over (nonce, gasprice, startgas, to, value, data, chainid, 0, 0) and set v = {0,1} + CHAIN_ID*2 + 35. Mainnet CHAIN_ID = 1. #Cryptography #Blockchain

Source : https://github.com/ethereum/EIPs/blob/master/EIPS/eip-155.md

### 43/ · checked · choix :

**43a (simple)** : #Crypto Math #43: When a dApp asks for a signature, EIP-712 lets your wallet show readable fields instead of a blob of hex. Read them before you sign. #Cryptography #Blockchain

**43b (tech)** : #Crypto Math #43: EIP-712 signs typed structured data, not raw bytes: \x19\x01 || domainSeparator || hashStruct(message). The domain separator ties a signature to one app and contract. #Cryptography #Blockchain

Source : https://github.com/ethereum/EIPs/blob/master/EIPS/eip-712.md

### 44/ · checked · choix :

**44a (simple)** : #Crypto Math #44: A 12-word seed phrase holds 128 bits of randomness plus a 4-bit checksum. That's why 12 random words are rarely a valid phrase: only about 1 in 16 is. #Cryptography #Blockchain

**44b (tech)** : #Crypto Math #44: BIP39: entropy ENT + checksum = first ENT/32 bits of SHA256(entropy). 12 words: 128 + 4 = 132 bits = 12 x 11. So 1 in 16 random 12-word combos is valid. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki

### 45/ · checked · choix :

**45a (simple)** : #Crypto Math #45: A 24-word seed holds 256 bits of randomness plus an 8-bit checksum. The 24th word is mostly checksum: it carries only 3 bits of randomness. #Cryptography #Blockchain

**45b (tech)** : #Crypto Math #45: BIP39 24 words: 256-bit entropy + 8-bit checksum = 264 bits = 24 x 11. The last word = 3 entropy bits + 8 checksum bits, so only 8 of 2048 words can complete a given 23-word start. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki

### 46/ · checked · choix :

**46a (simple)** : #Crypto Math #46: The seed phrase word list has exactly 2048 words. Each word encodes 11 bits, because 2^11 = 2048. #Cryptography #Blockchain

**46b (tech)** : #Crypto Math #46: BIP39 splits entropy + checksum into 11-bit groups, each an index 0-2047 into a 2048-word list. 2048^12 = 2^132. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki

### 47/ · checked · choix :

**47a (simple)** : #Crypto Math #47: In the English seed word list, the first 4 letters of each word are unique. Write down just 4 letters and the word can still be recovered. #Cryptography #Blockchain

**47b (tech)** : #Crypto Math #47: BIP39 English wordlist: 2048 words, no two share the same first 4 letters, so a 4-letter prefix identifies the word. Similar words are also avoided to limit transcription errors. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0039/english.txt

### 48/ · checked · choix :

**48a (simple)** : #Crypto Math #48: The seed checksum is short. BIP39's own text says it gives modest odds of catching random errors, and it can't tell you which word is wrong. #Cryptography #Blockchain

**48b (tech)** : #Crypto Math #48: BIP39 'shortcomings': the checksum is short; with 8 bits (24 words), 1 in 256 random errors is missed. With 12 words (4 bits) it's 1 in 16. No error correction. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki

### 49/ · checked · choix :

**49a (simple)** : #Crypto Math #49: Your seed phrase isn't used as-is. It goes through 2048 rounds of hashing to become a 512-bit master seed, which makes guessing slower. #Cryptography #Blockchain

**49b (tech)** : #Crypto Math #49: BIP39 seed = PBKDF2 with HMAC-SHA512, password = mnemonic (UTF-8 NFKD), salt = 'mnemonic' + passphrase, 2048 iterations. Output: a 512-bit seed. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki

### 50/ · checked · choix :

**50a (simple)** : #Crypto Math #50: A seed passphrase (the '25th word') opens a completely different wallet. Any passphrase is valid, so a wrong one shows an empty wallet, never an error. #Cryptography #Blockchain

**50b (tech)** : #Crypto Math #50: BIP39: the passphrase goes into the PBKDF2 salt. Every passphrase yields a valid seed: plausible deniability, but a typo silently derives a different wallet. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki

### 51/ · checked · choix :

**51a (simple)** : #Crypto Math #51: One seed can produce billions of addresses. Step one: hash the seed with the fixed phrase 'Bitcoin seed' to get a master key. #Cryptography #Blockchain

**51b (tech)** : #Crypto Math #51: BIP32: I = HMAC-SHA512(key = 'Bitcoin seed', data = seed). Left 32 bytes = master private key, right 32 bytes = chain code (an extra 256 bits of entropy). #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki

### 52/ · checked · choix :

**52a (simple)** : #Crypto Math #52: Each key in an HD wallet has about 4.3 billion possible children: half 'normal', half 'hardened'. Hardened ones limit the damage if a key leaks. #Cryptography #Blockchain

**52b (tech)** : #Crypto Math #52: BIP32: each extended key has 2^31 normal children (index 0 to 2^31-1) and 2^31 hardened (2^31 to 2^32-1). Hardened derivation hashes the parent private key, not the public key. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki

### 53/ · checked · choix :

**53a (simple)** : #Crypto Math #53: Sharing your xpub only reveals balances, right? Unless one child private key also leaks. Then an attacker can rebuild every key under that xpub. #Cryptography #Blockchain

**53b (tech)** : #Crypto Math #53: BIP32 warning: parent xpub + any non-hardened child private key = the parent extended private key, and thus every key below it. Account levels use hardened derivation for this reason. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki

### 54/ · checked · choix :

**54a (simple)** : #Crypto Math #54: That string m/44'/0'/0'/0/0 in wallet settings is a path: it tells the wallet which branch of its key tree to use. The ' marks a hardened step. #Cryptography #Blockchain

**54b (tech)** : #Crypto Math #54: BIP44 path: m / purpose' / coin_type' / account' / change / address_index. Purpose 44, coin type 0 = Bitcoin. Same seed + same path = same address in any compatible wallet. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki

### 55/ · checked · choix :

**55a (simple)** : #Crypto Math #55: You can split a seed into shares, say any 3 of 5. With fewer than 3 shares, an attacker learns nothing about the seed. That's Shamir's secret sharing. #Cryptography #Blockchain

**55b (tech)** : #Crypto Math #55: SLIP-39: Shamir's secret sharing for mnemonics. Shares use a 1024-word list (10 bits per word) and an RS1024 checksum (last 3 words). Fewer than the threshold leaks nothing. #Cryptography #Blockchain

Source : https://github.com/satoshilabs/slips/blob/master/slip-0039.md

### 56/ · checked · choix :

**56a (simple)** : #Crypto Math #56: Silent Payments let you publish one static address, yet every payment lands at a fresh address that only you can find. #Cryptography #Blockchain

**56b (tech)** : #Crypto Math #56: BIP352: the sender tweaks the output with ECDH (a*B = b*A) over the sum of input keys. The receiver scans for P = B + hash(b*A)*G. No on-chain notification, no linkability. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0352.mediawiki

### 57/ · checked · choix :

**57a (simple)** : #Crypto Math #57: A block's transactions are summed up in one hash, the Merkle root. With an odd count, Bitcoin copies the last hash, a quirk that once enabled a bug. #Cryptography #Blockchain

**57b (tech)** : #Crypto Math #57: Bitcoin's Merkle tree duplicates the last hash on odd levels, so some tx lists give the same root: CVE-2012-2459 (blocks with duplicate txids). Nodes now detect this mutation. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bitcoin/blob/master/src/consensus/merkle.cpp

### 58/ · checked · choix :

**58a (simple)** : #Crypto Math #58: To prove a payment is in a block of about 4,000 transactions, you need only about 12 hashes, not the whole block. That's how light wallets check payments. #Cryptography #Blockchain

**58b (tech)** : #Crypto Math #58: Merkle proofs are log2(n) hashes: a block of 4,096 txs needs 12 sibling hashes (12 x 32 = 384 bytes) to link a txid to the header's Merkle root. Basis of SPV. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bitcoin/blob/master/src/consensus/merkle.cpp

### 59/ · checked · choix :

**59a (simple)** : #Crypto Math #59: 'Brain wallets' turned a memorable phrase into a private key. Attackers just hashed huge lists of phrases and swept the funds. Humans make bad random generators. #Cryptography #Blockchain

**59b (tech)** : #Crypto Math #59: Brainwallet: private key = SHA256(passphrase). Ryan Castellucci's DEF CON 23 talk (2015) and his Brainflayer tool showed how cheaply such keys are cracked at scale. #Cryptography #Blockchain

Source : https://infocondb.org/con/def-con/def-con-23/cracking-cryptocurrency-brainwallets

### 60/ · checked · choix :

**60a (simple)** : #Crypto Math #60: Milk Sad (2023): a command-line wallet tool built seed phrases from a 32-bit clock-based seed. Only about 4 billion possible wallets, all crackable. Over $900K was stolen. #Cryptography #Blockchain

**60b (tech)** : #Crypto Math #60: Milk Sad, CVE-2023-39910: Libbitcoin Explorer 3.x 'bx seed' used Mersenne Twister MT19937 seeded with 32 bits of clock time, so about 2^32 keys. About 29.65 BTC stolen on Jul 12, 2023. #Cryptography #Blockchain

Source : https://git.distrust.co/milksad/website-public/src/branch/main-public/disclosure.md

### 61/ · checked · choix :

**61a (simple)** : #Crypto Math #61: Randstorm: browser wallets made around 2011-2015 may have weak keys, because an old JavaScript library quietly fell back to a weak random function. #Cryptography #Blockchain

**61b (tech)** : #Crypto Math #61: Randstorm: JSBN SecureRandom() (in BitcoinJS until Mar 2014) looked for window.crypto.random, missing in browsers, and silently fell back to Math.random(). Wallets from about 2011-2015. #Cryptography #Blockchain

Source : https://www.unciphered.com/blog/randstorm-you-cant-patch-a-house-of-cards

### 62/ · checked · choix :

**62a (simple)** : #Crypto Math #62: In 2022, a wallet browser extension generated seeds with only 32 bits of randomness. Knowing an address was enough to rebuild its key. Ledger's researchers caught it. #Cryptography #Blockchain

**62b (tech)** : #Crypto Math #62: Trust Wallet extension (Nov 2022): seed entropy was effectively 32 bits, so every seed could be enumerated. Found by Ledger Donjon, fix committed Nov 21, 2022, disclosed Apr 22, 2023. #Cryptography #Blockchain

Source : https://nobsbitcoin.com/trust-wallet-browser-extension-vulnerability

### 63/ · checked · choix :

**63a (simple)** : #Crypto Math #63: Vanity addresses look cool (0x0000...), but in 2022 a flawed generator, Profanity, made keys that could be brute-forced. Market maker Wintermute lost $160M. #Cryptography #Blockchain

**63b (tech)** : #Crypto Math #63: Sep 20, 2022: Wintermute lost $160M from a vault whose admin was a Profanity vanity address (0x0000000...). 1inch had just disclosed that Profanity keys could be brute-forced. #Cryptography #Blockchain

Source : https://www.theblock.co/post/171192

### 64/ · checked · choix :

**64a (simple)** : #Crypto Math #64: In 2017, Monero found a bug that could have let someone create unlimited coins undetected. It was patched quietly first; Monero's chain was never exploited. #Cryptography #Blockchain

**64b (tech)** : #Crypto Math #64: Monero (found Feb 19, 2017): adding a small-order (torsion) point to an ed25519 key image let one output be spent up to 8 times. Fix: check that key images lie in the prime-order subgroup. #Cryptography #Blockchain

Source : https://getmonero.org/2017/05/17/disclosure-of-a-major-bug-in-cryptonote-based-currencies.html

### 65/ · checked · choix :

**65a (simple)** : #Crypto Math #65: Ethereum validators don't sign with secp256k1. They use BLS signatures on the BLS12-381 curve, which can be merged together. #Cryptography #Blockchain

**65b (tech)** : #Crypto Math #65: Ethereum consensus uses BLS12-381: public keys are 48-byte compressed G1 points, signatures 96-byte compressed G2 points. Verification uses pairings. #Cryptography #Blockchain

Source : https://github.com/ethereum/consensus-specs/blob/master/specs/phase0/beacon-chain.md

### 66/ · checked · choix :

**66a (simple)** : #Crypto Math #66: BLS signatures add up: hundreds of Ethereum validators can sign, and the result is a single signature checked in one go. #Cryptography #Blockchain

**66b (tech)** : #Crypto Math #66: BLS aggregation: signatures on the same message combine into one 96-byte signature, verified against the aggregate pubkey. Altair's sync committee: 512 validators. #Cryptography #Blockchain

Source : https://github.com/ethereum/consensus-specs/blob/master/specs/altair/beacon-chain.md

### 67/ · checked · choix :

**67a (simple)** : #Crypto Math #67: Ethereum validator keys hide a quantum plan B: their derivation goes through hash-based Lamport keys, meant as a bridge if BLS ever breaks. #Cryptography #Blockchain

**67b (tech)** : #Crypto Math #67: EIP-2333 (BLS12-381 key derivation): each child key is derived via intermediate Lamport keys, a hash-based scheme meant as a post-quantum backup if BLS12-381 is no longer secure. #Cryptography #Blockchain

Source : https://github.com/ethereum/ercs/blob/master/ERCS/erc-2333.md

### 68/ · checked · choix :

**68a (simple)** : #Crypto Math #68: Ethereum has built-in shortcuts for the curve math used by zero-knowledge proofs. Without them, checking such a proof would cost too much gas. #Cryptography #Blockchain

**68b (tech)** : #Crypto Math #68: EIP-196/197 added precompiles for alt_bn128 (BN254) point addition, scalar multiplication and pairing checks, so zkSNARKs can be verified within the block gas limit. #Cryptography #Blockchain

Source : https://github.com/ethereum/EIPs/blob/master/EIPS/eip-196.md

### 69/ · checked · choix :

**69a (simple)** : #Crypto Math #69: In 2018, a Zcash engineer found a flaw in its zero-knowledge proof system that could have allowed invisible counterfeiting. Fixed in Oct 2018, made public in Feb 2019. #Cryptography #Blockchain

**69b (tech)** : #Crypto Math #69: Zcash counterfeiting bug in its pre-Sapling zk-SNARK system: found by Ariel Gabizon on Mar 1, 2018, fixed by Sapling (Oct 28, 2018), disclosed Feb 5, 2019. No evidence of exploitation. #Cryptography #Blockchain

Source : https://fortune.com/2019/02/05/zcash-vulnerability-cryptocurrency

### 70/ · checked · choix :

**70a (simple)** : #Crypto Math #70: Ethereum's 2023 setup ceremony collected 141,416 contributions. It stays secure as long as just one participant was honest and destroyed their secret. #Cryptography #Blockchain

**70b (tech)** : #Crypto Math #70: KZG ceremony, Jan 13 to Aug 8, 2023: 141,416 contributions over 208 days, for EIP-4844 blob commitments. 1-of-N trust: one honest contributor is enough. #Cryptography #Blockchain

Source : https://blog.ethereum.org/2024/01/23/kzg-wrap

### 71/ · checked · choix :

**71a (simple)** : #Crypto Math #71: Ethereum 'blobs' carry data for layer-2 networks. Each blob gets a short cryptographic commitment, so nodes can check the data without trusting anyone. #Cryptography #Blockchain

**71b (tech)** : #Crypto Math #71: EIP-4844 blob = 4096 BLS12-381 field elements (32 bytes each, 128 KiB), with a KZG commitment. Txs reference it by versioned hash: 0x01 || SHA256(commitment)[1:]. #Cryptography #Blockchain

Source : https://github.com/ethereum/EIPs/blob/master/EIPS/eip-4844.md

### 72/ · checked · choix :

**72a (simple)** : #Crypto Math #72: The quantum threat in short: Shor's algorithm, on a large enough quantum computer, could derive a private key from a public key. Nobody knows when or if such a machine will exist. #Cryptography #Blockchain

**72b (tech)** : #Crypto Math #72: Shor's algorithm solves the discrete log problem exponentially faster than classical methods, so a cryptographically relevant quantum computer (CRQC) could derive ECDSA/Schnorr keys from public keys. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0360.mediawiki

### 73/ · checked · choix :

**73a (simple)** : #Crypto Math #73: Not all coins face the same quantum risk. Coins whose public key is already on-chain, like Satoshi-era coins, could be attacked at leisure. #Cryptography #Blockchain

**73b (tech)** : #Crypto Math #73: BIP360 'long exposure' attacks target keys already public on-chain: P2PK outputs (e.g. Satoshi's coins), reused addresses, Taproot outputs. 'Short exposure' must beat confirmation time. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0360.mediawiki

### 74/ · checked · choix :

**74a (simple)** : #Crypto Math #74: Quantum-safe signatures already exist on paper. NIST published its first post-quantum standards in August 2024, two of them for digital signatures. #Cryptography #Blockchain

**74b (tech)** : #Crypto Math #74: Aug 13, 2024: NIST released FIPS 203 (ML-KEM, ex-Kyber), FIPS 204 (ML-DSA, ex-Dilithium) and FIPS 205 (SLH-DSA, ex-SPHINCS+). 204 and 205 are signature schemes. #Cryptography #Blockchain

Source : https://www.nist.gov/news-events/news/2024/08/nist-releases-first-3-finalized-post-quantum-encryption-standards

### 75/ · checked · choix :

**75a (simple)** : #Crypto Math #75: One Bitcoin quantum proposal, BIP360 (P2MR), is Taproot without the public-key shortcut, so keys aren't left exposed on-chain. It's still a draft. #Cryptography #Blockchain

**75b (tech)** : #Crypto Math #75: BIP360 (Draft; P2QRH, then P2TSH, renamed P2MR in Feb 2026): Taproot-like outputs with the key-path spend removed, resisting long-exposure quantum attacks. Short exposure needs PQ signatures. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips/blob/master/bip-0360.mediawiki

### 76/ · checked · choix :

**76a (simple)** : #Crypto Math #76: Recap: your coins rest on a random number, one curve, a few hash functions and signatures. The math holds up; losses come from bad randomness and sloppy code. Follow for more. #Cryptography #Blockchain

**76b (tech)** : #Crypto Math #76: Recap: secp256k1 ECDLP (128-bit security), SHA-256 and Keccak, ECDSA then Schnorr, BLS aggregation, BIP39/32 seeds, ZK proofs. Most real losses: nonce reuse and weak RNGs. Follow for more. #Cryptography #Blockchain

Source : https://github.com/bitcoin/bips
