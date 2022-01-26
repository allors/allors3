import { Composite } from '@allors/system/workspace/meta';
import { FilterDefinition } from '../filter/filter-definition';

interface AngularFilterDefinitionExtension {
  filterDefinition?: FilterDefinition;
}

export function angularFilterDefinition(composite: Composite): FilterDefinition;
export function angularFilterDefinition(
  composite: Composite,
  filterDefinition: FilterDefinition
): void;
export function angularFilterDefinition(
  composite: Composite,
  filterDefinition?: FilterDefinition
): FilterDefinition | void {
  const extension = composite?._ as AngularFilterDefinitionExtension;

  if (filterDefinition == null) {
    return extension?.filterDefinition;
  }

  extension.filterDefinition = filterDefinition;
}
