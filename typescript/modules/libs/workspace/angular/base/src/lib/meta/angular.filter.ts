import { Composite } from '@allors/workspace/meta/system';
import { Filter } from '../components/filter/filter';

interface AngularFilterExtension {
  filter?: Filter;
}

export function angularFilter(composite: Composite, filter?: Filter) {
  if (filter == null) {
    return (composite._ as AngularFilterExtension).filter;
  }

  (composite._ as AngularFilterExtension).filter = filter;
}
