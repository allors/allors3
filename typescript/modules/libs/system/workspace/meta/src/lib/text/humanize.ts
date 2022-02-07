export function humanize(input?: string): string | null {
  if (!input) {
    return null;
  }

  let result = input
    .replace(/([a-z\d])([A-Z])/g, '$1 $2')
    .replace(/([A-Z]+)([A-Z][a-z\d]+)/g, '$1 $2');

  result = result.charAt(0).toUpperCase() + result.slice(1);
  return result;
}
