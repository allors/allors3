import { IRange, Ranges } from './ranges';

/**
 * The ids the wire delivered as a range: sorted, and undefined when there are none, whether
 * the wire sent null or an empty array.
 */
export function loadRange(
  ranges: Ranges<number>,
  ids: number[] | null | undefined
): IRange<number> {
  const range = ranges.importFrom(ids);
  return range?.length > 0 ? range : undefined;
}
