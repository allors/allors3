import { Composite } from '@allors/system/workspace/meta';
import { Sorter } from '../sort/sorter';

interface AngularSorterExtension {
  sorter?: Sorter;
}

export function angularSorter(composite: Composite): Sorter;
export function angularSorter(composite: Composite, sorter: Sorter): void;
export function angularSorter(
  composite: Composite,
  sorter?: Sorter
): Sorter | void {
  const extension = composite?._ as AngularSorterExtension;

  if (sorter == null) {
    return extension?.sorter;
  }

  extension.sorter = sorter;
}
