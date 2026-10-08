import { MetaPopulation } from '@allors/system/workspace/meta';

// FNV-1a, 64 bit, in four 16-bit limbs, least significant first, so that it needs neither
// BigInt nor a 64-bit integer: a product of a limb and a limb of the prime stays well below
// 2^53, and the carries are taken limb by limb.
const OFFSET_BASIS = [0x2325, 0x8422, 0x9ce4, 0xcbf2];

// The prime 0x100000001b3: 0x01b3 in the lowest limb and 0x0100 in the third.
const PRIME_LOW = 0x01b3;
const PRIME_HIGH = 0x0100;

/**
 * The fingerprint of a meta population: the hash of the tags of its composites, relation
 * types and method types, sorted, so that the server and a client that generated its
 * workspace meta from the same repository compute the same value, whatever the order of
 * their collections. FNV-1a, 64 bit, over the UTF-8 bytes of each tag followed by a line
 * feed, as sixteen lowercase hex characters; the .NET MetaFingerprint computes the same.
 */
export function metaFingerprint(tags: Iterable<string>): string {
  if (tags == null) {
    throw new Error('A fingerprint needs the tags to hash.');
  }

  const sorted = [...tags].sort(ordinal);
  const hash = [...OFFSET_BASIS];

  for (const tag of sorted) {
    for (const byte of utf8(tag)) {
      mix(hash, byte);
    }

    mix(hash, 0x0a);
  }

  return hash
    .slice()
    .reverse()
    .map((limb) => limb.toString(16).padStart(4, '0'))
    .join('');
}

/**
 * The fingerprint of the workspace meta: metaFingerprint over the tags of its composites,
 * relation types and method types, the same members the server hashes for the workspace
 * the meta was generated for.
 */
export function metaPopulationFingerprint(metaPopulation: MetaPopulation): string {
  if (metaPopulation == null) {
    throw new Error('A fingerprint needs a meta population.');
  }

  return metaFingerprint([
    ...[...metaPopulation.composites].map((v) => v.tag),
    ...[...metaPopulation.relationTypes].map((v) => v.tag),
    ...[...metaPopulation.methodTypes].map((v) => v.tag),
  ]);
}

// hash = (hash ^ byte) * prime, mod 2^64.
function mix(hash: number[], byte: number): void {
  hash[0] ^= byte;

  const [h0, h1, h2, h3] = hash;
  const products = [
    h0 * PRIME_LOW,
    h1 * PRIME_LOW,
    h2 * PRIME_LOW + h0 * PRIME_HIGH,
    h3 * PRIME_LOW + h1 * PRIME_HIGH,
  ];

  let carry = 0;
  for (let i = 0; i < 4; i++) {
    const sum = products[i] + carry;
    hash[i] = sum & 0xffff;
    carry = Math.floor(sum / 0x10000);
  }
}

// The ordinal order of .NET's StringComparer.Ordinal: by UTF-16 code unit.
function ordinal(a: string, b: string): number {
  return a < b ? -1 : a > b ? 1 : 0;
}

// The UTF-8 bytes of a string, as Encoding.UTF8 gives them; written out so that it needs
// no TextEncoder, which not every test environment has.
function* utf8(text: string): Iterable<number> {
  for (const character of text) {
    const codePoint = character.codePointAt(0);

    if (codePoint < 0x80) {
      yield codePoint;
    } else if (codePoint < 0x800) {
      yield 0xc0 | (codePoint >> 6);
      yield 0x80 | (codePoint & 0x3f);
    } else if (codePoint < 0x10000) {
      yield 0xe0 | (codePoint >> 12);
      yield 0x80 | ((codePoint >> 6) & 0x3f);
      yield 0x80 | (codePoint & 0x3f);
    } else {
      yield 0xf0 | (codePoint >> 18);
      yield 0x80 | ((codePoint >> 12) & 0x3f);
      yield 0x80 | ((codePoint >> 6) & 0x3f);
      yield 0x80 | (codePoint & 0x3f);
    }
  }
}
