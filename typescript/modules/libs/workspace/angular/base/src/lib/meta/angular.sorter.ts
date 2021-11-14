import { Composite } from '@allors/workspace/meta/system';
import { Sorter } from '../material/sorting/sorter';

interface AngularSorterExtension {
  sorter?: Sorter;
}

export function angularSorter(composite: Composite, sorter?: Sorter) {
  if (sorter == null) {
    return (composite._ as AngularSorterExtension).sorter;
  }

  (composite._ as AngularSorterExtension).sorter = sorter;
}
