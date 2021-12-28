import { Composite } from '@allors/workspace/meta/system';
import { Sorter } from '../material/sorting/sorter';

interface AngularSorterExtension {
  sorter?: Sorter;
}

export function angularSorter(composite: Composite): Sorter;
export function angularSorter(composite: Composite, sorter: Sorter): void;
export function angularSorter(composite: Composite, sorter?: Sorter): Sorter | void {
  if (composite == null) {
    return;
  }

  if (sorter == null) {
    return (composite._ as AngularSorterExtension).sorter;
  }

  (composite._ as AngularSorterExtension).sorter = sorter;
}
