import { Composite } from '@allors/workspace/meta/system';
import { FilterDefinition } from '../components/filter/filter-definition';

interface AngularFilterDefinitionExtension {
  filterDefinition?: FilterDefinition;
}

export function angularFilterDefinition(composite: Composite, filterDefinition?: FilterDefinition) {
  if (filterDefinition == null) {
    return (composite._ as AngularFilterDefinitionExtension).filterDefinition;
  }

  (composite._ as AngularFilterDefinitionExtension).filterDefinition = filterDefinition;
}
