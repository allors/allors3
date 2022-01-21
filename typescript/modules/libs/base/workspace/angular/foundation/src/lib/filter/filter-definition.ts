import {
  Predicate,
  Extent,
  ParameterizablePredicate,
} from '@allors/system/workspace/domain';

import { FilterOptions } from './filter-options';
import { FilterFieldDefinition } from './filter-field-definition';

function parametrize(
  predicate: Predicate | Extent,
  results: ParameterizablePredicate[] = []
): ParameterizablePredicate[] {
  switch (predicate.kind) {
    case 'Filter':
      // case 'Union':
      // case 'Intersect':
      // case 'Except':
      if (predicate.predicate) {
        parametrize(predicate.predicate, results);
      }
      break;

    case 'And':
    case 'Or':
      if (predicate.operands) {
        predicate.operands.forEach((v) => parametrize(v, results));
      }
      break;

    case 'Not':
      if (predicate.operand) {
        parametrize(predicate.operand, results);
      }
      break;

    case 'ContainedIn':
      if (predicate.extent) {
        parametrize(predicate.extent, results);
      }
      break;
  }

  if ((predicate as ParameterizablePredicate).parameter) {
    results.push(predicate as ParameterizablePredicate);
  }

  return results;
}

export class FilterDefinition {
  fieldDefinitions: FilterFieldDefinition[];

  constructor(
    public predicate: Predicate,
    options?: { [parameter: string]: Partial<FilterOptions> }
  ) {
    const predicates = parametrize(predicate);
    this.fieldDefinitions = predicates.map(
      (v) =>
        new FilterFieldDefinition({
          predicate: v,
          options:
            options && v.parameter
              ? new FilterOptions(options[v.parameter])
              : undefined,
        })
    );
  }
}
