import { Composite } from '@allors/workspace/meta/system';
import { FilterDefinition } from '../filter/filter-definition';

interface AngularFilterDefinitionExtension {
  filterDefinition?: FilterDefinition;
}

export function angularFilterDefinition(composite: Composite): FilterDefinition;
export function angularFilterDefinition(composite: Composite, filterDefinition: FilterDefinition): void;
export function angularFilterDefinition(composite: Composite, filterDefinition?: FilterDefinition): FilterDefinition | void {
  if (composite == null) {
    return;
  }

  if (filterDefinition == null) {
    return (composite._ as AngularFilterDefinitionExtension).filterDefinition;
  }

  (composite._ as AngularFilterDefinitionExtension).filterDefinition = filterDefinition;
}
