import { metaFingerprint } from '@allors/system/workspace/connection';

// The fingerprint of a meta population is the same on every side that computes it from the
// same tags, whatever their order, and differs for any other set of tags. The reference
// values are those of the .NET MetaFingerprintTests, so that both sides agree.
describe('metaFingerprint', () => {
  it('is sixteen lowercase hex characters', () => {
    expect(metaFingerprint(['a', 'b'])).toMatch(/^[0-9a-f]{16}$/);
    expect(metaFingerprint([])).toMatch(/^[0-9a-f]{16}$/);
  });

  it('is the same for the same tags in any order', () => {
    const tags = ['h6OmCbWhOECwdDoByBy9og', '0gIHQyvgrUWbIrgzHcdaPw', '1', '7'];

    const fingerprint = metaFingerprint(tags);

    expect(metaFingerprint([...tags].reverse())).toBe(fingerprint);
    expect(metaFingerprint([...tags].sort())).toBe(fingerprint);
    expect(metaFingerprint(new Set(tags))).toBe(fingerprint);
  });

  it('differs when a tag is added, removed or changed', () => {
    const fingerprint = metaFingerprint(['a', 'b', 'c']);

    expect(metaFingerprint(['a', 'b'])).not.toBe(fingerprint);
    expect(metaFingerprint(['a', 'b', 'c', 'd'])).not.toBe(fingerprint);
    expect(metaFingerprint(['a', 'b', 'x'])).not.toBe(fingerprint);
    expect(metaFingerprint(['ab', 'c'])).not.toBe(fingerprint);
  });

  it('is a known value so that every implementation agrees', () => {
    // FNV-1a, 64 bit, over the UTF-8 bytes of the sorted tags, each followed by a line feed;
    // the .NET MetaFingerprint computes the same.
    expect(metaFingerprint([])).toBe('cbf29ce484222325');
    expect(metaFingerprint(['a'])).toBe('089bdc07b544e7b2');
    expect(metaFingerprint(['b', 'a'])).toBe('78ed6781f136a14e');
  });

  it('hashes the UTF-8 bytes of a tag beyond ASCII', () => {
    // Two tags that differ only beyond ASCII hash differently, and a tag hashes as its bytes,
    // not as its UTF-16 code units.
    expect(metaFingerprint(['ᴀbra'])).not.toBe(metaFingerprint(['abra']));
    expect(metaFingerprint(['é'])).not.toBe(metaFingerprint(['é']));
  });
});
