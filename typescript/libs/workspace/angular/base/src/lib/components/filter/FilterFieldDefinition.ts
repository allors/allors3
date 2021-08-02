import { humanize } from '@allors/workspace/meta/system';
import { ParameterizablePredicate, Like, Equals, Exists, Between } from '@allors/workspace/domain/system';

import { FilterOptions } from './FilterOptions';

export class FilterFieldDefinition {
  predicate: ParameterizablePredicate;
  options?: FilterOptions;

  get isLike() {
    return this.predicate instanceof Like;
  }

  get isExists() {
    return this.predicate instanceof Exists;
  }

  get isBetween() {
    return this.predicate instanceof Between;
  }

  get name(): string | undefined {
    return this.predicate.parameter ? humanize(this.predicate.parameter) : undefined;
  }

  get criteria(): string {
    if (this.isLike) {
      return 'Starts with';
    }

    return 'Select';
  }

  constructor(fields?: Partial<FilterFieldDefinition>) {
    Object.assign(this, fields);
  }
}
