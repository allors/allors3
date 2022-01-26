import { Composite } from '@allors/system/workspace/meta';
import { Filter } from '../filter/filter';
import { angularFilterDefinition } from './angular-filter-definition';

interface AngularFilterExtension {
  filter?: Filter;
}

export function angularFilter(composite: Composite): Filter;
export function angularFilter(composite: Composite, filter: Filter): void;
export function angularFilter(
  composite: Composite,
  filter?: Filter
): Filter | void {
  const extension = composite._ as AngularFilterExtension;

  if (filter == null) {
    return extension.filter;
  }

  extension.filter = filter;
}

export function angularFilterFromDefinition(composite: Composite): Filter {
  let filter = angularFilter(composite);
  if (filter == null) {
    filter = new Filter(angularFilterDefinition(composite));
    angularFilter(composite, filter);
  }

  return filter;
}
