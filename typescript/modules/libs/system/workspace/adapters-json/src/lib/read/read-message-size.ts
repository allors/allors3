/** Counts UTF-8 without requiring Node APIs in browser workspace consumers. */
export function jsonBytes(json: string): number {
  let count = 0;
  for (let i = 0; i < json.length; i++) {
    const code = json.charCodeAt(i);
    if (code < 0x80) count++;
    else if (code < 0x800) count += 2;
    else if (
      code >= 0xd800 &&
      code <= 0xdbff &&
      i + 1 < json.length &&
      json.charCodeAt(i + 1) >= 0xdc00 &&
      json.charCodeAt(i + 1) <= 0xdfff
    ) {
      count += 4;
      i++;
    } else count += 3;
  }
  return count;
}
