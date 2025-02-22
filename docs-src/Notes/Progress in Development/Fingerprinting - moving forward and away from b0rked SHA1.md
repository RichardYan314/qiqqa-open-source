# Fingerprinting :: moving forward and away from b0rked SHA1

Okay, it's a known fact that the SHA1-based fingerprinting used in qiqqa to identify PDF documents is flawed: that's a bug that exists since the inception of the software. The SHA1 binary hash was encoded as HEX, but any byte value with a zero(0) for the most significant nibble would have that nibble silently discarded.

This results in a couple of things, none of them major, but enough for me to consider moving:

- the SHA1 hash is thus corrupted as-useed in Qiqqa: the nibble sequence `0A BC 0D` in a hash is indistinguishable from `AB 0C 0D` and `0A 0B CD` as all encode as `ABCD`. Thus the number of 'bits' in the hash are reduced quite a bit - not enough to cause immediate disaster, but there's more risk at collisions as the hash is not used the way it should.
- if I want to store both SHA1-colliding PDFs as separate documents, I can't. That's not important if I only store research papers, but when you are stretching the original goals of Qiqqa to suit your needs, you're in trouble.
 

Given the b0rked SHA1 encoding in Qiqqa, you can expect more than 1 collision out there as the number of hash bits is close to *halved* due to the encode b0rk, going from 160bits to ~80 bits and that's getting close to "hmmm, not feeling all that safe anymore" chances: when looking at the [Birthday Paradox Probability Table](https://en.wikipedia.org/wiki/Birthday_problem#Probability_table) I'm aiming for a fingerprint that can virtually *guarantee* uniqueness of fingerprint per document, even in [*adverse circumstances*](https://alf.nu/SHA1). In that table, I'm therefor only interested in the $p <= 10^-18$ column as I'm risk averse like that. (More info about [SHAttered](https://shattered.io/) and [related](https://people.csail.mit.edu/yiqun/SHA1AttackProceedingVersion.pdf) [material](https://www.theregister.com/2017/02/23/google_first_sha1_collision) [in](../assets/MD5_SHA1_transcript-collisions.pdf) [these](../assets/SHA1_Attack_Proceeding_Version.pdf) [here](../assets/Collisions_of_SHA_0_and_Reduced_SHA_1.pdf) [links](../assets/Collisions_of_SHA_0_and_Reduced_SHA_1-34940036.pdf).)

## So first question then is: can we grab ourselves another hash, which does better?

Sure we can. Plenty of them!


## But how about fingerprinting (i.e. *hashing*) *performance*?

Bigger hashes tend to get slower. but then I ran into this one: ["SHA512 faster than SHA256"](https://crypto.stackexchange.com/questions/26336/sha-512-faster-than-sha-256) and that got me looking for more.

There's also the article [Too Much Crypto by Aumassen](https://eprint.iacr.org/2019/1492.pdf), which addresses the uniqueness ("collision") challenge vs. performance.

Long story short: [BLAKE3](https://github.com/BLAKE3-team/BLAKE3) looks like a very promising hash, which is both better than SHA1 (so I'll be able to store *both* those SHA1-colliding PDF documents, which would *fail* if we were to merely *fix* the current SHA1 fingerprint encoding in Qiqqa) *plus* it's expected to be *faster* :yay:.


## Okay, that's the easy bit then. How do we remain backwards compatible or migrating forward?

Except for the colliding PDFs, the easy answer to that one would be to maintain a separate 1:1 lookup table in SQlite where old SHA1-B0RK fingerprints are mapped to new BLAKE3 hashes.

It would double the fingerprinting work as long as we want to stay backwards compatible as then we'ld have to calculate both BLAKE3 and SHA1-B0RK, but that's manageable. (text extraction tasks, etc. are more costly, so let's try to see this *relatively*: $\operatorname{fingerprinting}(D) * 2$ is not going to kill the app.


## Any fears about "cracking the hash" in the future? Other doubts & worries?

Not much. I'm not using the hash for pure cryptographic means, so a few odds and ends are not going to kill us. The fact that there exist real PDFs out there which collide on MD5 and SHA1 have made those hashes obsolete, but otherwise I believe we'll be doing fine with a reduced-strength, fast BLAKE3 for the foreseeable future.

Again, do realize we're not doing *crypto*/*security* here: the fingerprinting is also not employed in a *legal* setting where I need to unambiguously, beyond the shadow of doubt, *prove* that my fingerprint $F$ identifies document $D$.
The fact that I'm paranoid enough already (perfectionist?) means I'll tend to pick a fingerprint which has great expectations that way anyway and so far, BLAKE3 is looking great.

All this of course *assumes* I don't screw up the BLAKE3-based fingerprint (BLAKE3-Base58)

### Totally unrelated but still *related* is another matter: *near-identical* PDFs cluttering the library

As you asked about my doubt & worries, here's one: I discovered last week that https://www.researchgate.net/ does something nasty (from *my* perspective): it kind-of-*personalizes* your PDF download by editing its cover page: it brags about the number of downloads and the day you downloaded the PDF, so downloading the same document *twice* (say, via Qiqqa sniffer) will get you two(2) **different fingerprints**, thus the document downloads will be filed as unique documents in Qiqqa!

Sites which encode the date when you downloaded the PDF into the PDF itself (watermark/overlay) cause the same problem: multiple downloads cannot easily be identifies as duplicates through comparing their hashes!

That's therefor another **source of duplicates** for documents and increases the need for Qiqqa to incorporate better duplicate-detection logic (including 'probable cause of duplication' type identification, so you can observe & set *why* two documents are considered *duplicates*:

- watermark: time/otherwise-edited unique copy watermarks a la https://www.researchgate.net/
- website cover sheet: xArchiv and various universities or other sources may present the same content, but with different cover sheets. Again https://www.researchgate.net/ vs others.
- prepub vs. print: prepublication version vs print version. 
- journal, i.e. *which* print version: sometimes articles are published in multiple journals, each with their own formatting (and *editing*, sometimes). I'd say this type also includes "personal copies" as published by ohne of the authors.
- updates: xArchiv and a few others keep track of the *revisions*: v1, v2, v3, and so on of the published paper
- anniversary re-issues / reprints: some papers are reprinted after X years. Happens rarely, but they do exist. It happens more often when you include *books* in your library. *Editions* of books would, as far as I'm concerned, fall under the 'revisions/updates' type.
- amalgam/extract: sometimes papers are published in two versions: a short and a long one. Some papers are also *incorporated* into other papers, not just referenced, but re-used/quoted almost entirely, while other content is added. This goes beyond a mere 'update'.
- *plagiaat*: we now get into the more shady corners: some papers are copied or rephrased by others to make the publishing count ends meet. Plagiarism.
- *retractions*: that's a special kind of update, not really a *duplicate* per se, but more an *overriding* relationship. Remember our beautiful doctor who invented the pro-vegan argument of more aggression in meat-eating peeople while waiting for his train. Then the bugger went on to write a book about his life, earning yet more kudos. (I'm proud to be Dutch!)

and when we veer a little off from true duplicates that way, there's also:

- reviews of: reviews of a book or paper, either itself a paper or just a memo published in a journal or otherwise.
- meta-reviews: *Meta-reviews* are those where the "current state of art" is discussed and multiple papers are reviewed together to report on where we are right now as a group.
- answers to: memos and letters to the editor in answer to paper X or memo Y, that sort of thing mostly.
- replication reports (where B attempted to replicate A. Sometimes successful, sometimes not. Cold fusion by loose wiring, anyone?)
- bundling of (I have several PDFs which happen to contain multiple papers: that would be a *book* itself, but having that paper X in there would make it a duplicate of that paper (amalgam or otherwise)

Now I hope some day we'll be able to identify most, if not all, of those "duplicates" and "other relations" automatically...

Meanwhile, let's keep our fingerprint nicely unique and bemoan, yet accept/live with, the ways researchgate et al are thwarting our efforts (mostly *unintentionally*).






## References

- https://crypto.stackexchange.com/questions/26336/sha-512-faster-than-sha-256
- https://en.wikipedia.org/wiki/Birthday_attack
- https://eprint.iacr.org/2019/1492.pdf
- https://en.wikipedia.org/wiki/Birthday_problem#Probability_table
- https://stackoverflow.com/questions/62664761/probability-of-hash-collision
- https://en.wikipedia.org/wiki/BLAKE_(hash_function)

 



-------------

## Quick update

Both classical Qiqqa hash fingerprinting (the b0rky SHA1 algo) and a *new* one for future use, based on a fast BLAKE3 hash and Base58 encoding to string for generic use as filename, database key, etc., has been coded in my MuPDF-based toolkit for Qiqqa: https://github.com/GerHobbelt/mupdf/commit/e440b55474b288f9ff5127ee3bf35c67909ec858

Commit Message:

added two `mutool` utilities for Qiqqa:
- [`mutool qiqqa_fingerprint0`](https://github.com/GerHobbelt/mupdf/blob/master/source/tools/pdffingerprint0.cpp), which calculates the classic Qiqqa SHA1-b0rked fingerprint hash for any given (PDF) file
- [`mutool qiqqa_fingerprint1`](https://github.com/GerHobbelt/mupdf/blob/master/source/tools/pdffingerprint1.c), which calculates the *new* Qiqqa fingerprint, based on BLAKE3 and "tightening" by printing it as a Base58X rather then HEX fingerprint string. (I call this Base58X because it takes the tables off [Base58](https://tools.ietf.org/id/draft-msporny-base58-01.html) from the original bitcoin author Satoshi Nakamoto but then goes and does something completely different with it as the original bitcoin code would treat the hash-to-encode as one _BigInt_, which he then converted to Base58, but which takes quite a few divide and modulo ops, which I don't want to spend on that optimum result, so *instead* I look for where number base 58 and number base 2 get very close and that happens to be at bit/power 41. 
 
   This idea is very similar to what the folks of Base85/Ascii85 argued for: $(85^N \simeq 2^{32} \land 85^N \succ 2^{32})$, so their argument is that number base 85 (or higher) is very handy to use to encode 32-bit integer streams; *I* have looked at Base85 and Base91 and many others and I don't like them for the same reasons that are listed in [the Base58 header file by Satoshi Nakamoto](https://github.com/trottier/original-bitcoin/blob/master/src/base58.h): things get icky quickly – while having a slightly higher "encoding space efficiency" – as channels such as web (URLs, eMail, etc.), file systems (file names are pretty restricted outside modern UNIX file systems!), source code / JSON data files, etc.etc. all add their own quirks to such encodings, resulting in *variable length*, more-or-less-riddled-with-escapes-or-potential-faults-if-you-don't, encoded strings.

Base58 has the advantage of remaining a "selectable word" with nothing to get any interface medium's nickers in a twist either.

Besides, consider the relative gains (we're looking at *stringified* hashes as we'll be storing these in databases as *unique record keys* and other non-BLOB-allowing fields and thus do string-comparison based lookups and duplicate checks via $\textit{fingerprint} \stackrel{?}{=} \textit{record.hash}$ string compare operations):

#### String Encoding of a BLAKE3 full size hash: output size

| encoding     | calculus                   | # output chars        |
|--------------|----------------------------|-----------------------|
| HEX:         | $32 * 2 = 64$              | 64 chars              |
| Base64:      | $32 * 1.33 = 42.7$         | 43 chars              |
| Base85:      | $32 * (1/0.80) = 40$       | 40 chars              |
| Base91:      | $32 * (1/(1-0.23)) = 41.6$ | 42 chars (worst case) |
| Base91:      | $32 * (1/(1-0.14)) = 37.2$ | 38 chars (best case)  |
| Base58:      | $32 * (1/0.73) = 43.8$     | 44 chars              |
| **Base58X**: | $32 * 7 * 8 / 41 = 43.7$   | 44 chars too!         |

Notes on these datums:

- Base85 is 80% efficient according to Wikipedia: https://en.wikipedia.org/wiki/Binary-to-text_encoding
- Base91 efficiency numbers are from the source: http://base91.sourceforge.net/
- Base85 and Base91 output sizes (including the "*worst case*" numbers listed above!) DO NOT account for mandatory escaping of some of the output characters for various mediums (URL/Web, File Systems, etc.); these escapes can increase the output size cost to *twice*, possibly even *thrice* the listed size above (`%XX` URL encoding for some chars!) in rare circumstances, over which we have NO CONTROL: we can't tweak the hashes to "go around" these worst case scenarios lest we 'downgrade' to a Base58 approximation.
- Base58 (Bitcoin / Nakamoto) efficiency number according to Wikipedia: https://en.wikipedia.org/wiki/Binary-to-text_encoding
- Base58X: my own approach takes $58^7$ for every 41 bits, hence takes 7 output ASCII characters per 41 bits, hence a 32 *byte* hash takes $32 * 8 / 41$ chunks of 7 Blake48X output characters *each*, hence the total output size is $(32 * 8 / 41) * 7 \equiv 32 * 7 * 8 / 41 \simeq 43.7$ Base58X *characters*.
- Base58X: same output size as Base58 while I'll have far fewer divide and modulo ops than bitcoin Base58 (Nakamoto) code as I don't treat the entire hash as one *BigInt*, but work in an intermediate 41-bit number base system instead.

Anyway, all this fun is more suitable food for a blog article than a commit message   :-D



- https://crypto.bi/base58/
- https://en.wikipedia.org/wiki/Binary-to-text_encoding (and related pages on Wikipedia)
- https://crypto.stackexchange.com/questions/57580/purpose-of-folding-a-digest-in-half et al (I've been considering using a truncated or *folded* BLAKE3 hash to make the fingerprint string a little shorter, but decided against it in the end as it is not so important any more: the Base58X-encoded fingerprint strings clock in at 44 characters each, which is a $44 / (20 * 2) \Rightarrow 10\% \textit{ increase}$ in fingerprint hash string size compared to the original Qiqqa SHA1B (*B* for *B0rk*), while encoding $256 / 160 = 60\%$ more hash bits. 
 
  **Nitpicking**: Yes, SHA1B is variable length, but that's not under user or application control, merely an artifact of certain PDF data hashing results. The shortest SHA1B fingerprint in my collection is 36 characters, and that's *rare*: 5 items in over 20K documents. The *theoretical* minimum size of a SHA1B encoded hash would be 1 nibble per byte, hence 20 ASCII characters. The chance of getting one of those is $1 : 2^{(20 * 4)} \equiv 1 : 2^{80}$ which is *pretty rare* indeed. ;-)
- https://github.com/nakov/Practical-Cryptography-for-Developers-Book 


## Performance of the new vs. old hash: CPU load

Using the new [`mutool qiqqa_fingerprint0`](https://github.com/GerHobbelt/mupdf/blob/master/source/tools/pdffingerprint0.cpp) and [`mutool qiqqa_fingerprint1`](https://github.com/GerHobbelt/mupdf/blob/master/source/tools/pdffingerprint1.c) tools to calculate the hashes of a subset of the bulktest suite (~ 2K documents) the verdicts are: v2 (BLAKE3+B58X) is about as fast or even *up to 4 times faster* in execution time than SHA1B.

Of course one can argue this was not tested with the .NET version of the SHA1B code, but I expect that to be on par or even worse then the code I used for this, which is through use of the [Crypto++](https://github.com/weidai11/cryptopp) library, which is pretty performant and highly optimized generally.

Anyway, the take-away of this is that the new hash is *better* or at least *competitive* with the original Qiqqa hash, everything else being equal, and such has been my goal.

> Note: As the timings are all in the sub-second range, often only a couple of milliseconds, these are thus clearly within or near the noise margin of the timing measurement code, which has a millisecond *granularity* (Windows background tasks not related to the tests add an undetermined extra noise layer).
> 
> A quick check/sampling of the performance data for a large set of PDFs shows a performance factor [range](https://en.wikipedia.org/wiki/Range_(statistics)) of 0.9 .. 4 times *faster* than SHA1B (eyeballed mean speed factor somewhere around 1.5 to 2), using the same machinery (C code, optimized "Release Build" binaries used for running the performance benchmark).



