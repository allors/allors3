import { Composite } from '@allors/workspace/meta/system';
import { Filter } from '../filter/filter';
import { angularFilterDefinition } from './angular.filter.definition';

interface AngularFilterExtension {
  filter?: Filter;
}

export function angularFilter(composite: Composite): Filter;
export function angularFilter(composite: Composite, filter: Filter): void;
export function angularFilter(composite: Composite, filter?: Filter): Filter | void {
  if (composite == null) {
    return;
  }

  if (filter == null) {
    return (composite._ as AngularFilterExtension).filter;
  }

  (composite._ as AngularFilterExtension).filter = filter;
}

export function angularFilterFromDefinition(composite: Composite): Filter {
  let filter = angularFilter(composite);
  if (filter == null) {
    filter = new Filter(angularFilterDefinition(composite));
    angularFilter(composite, filter);
  }

  return filter;
}
