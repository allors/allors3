import { Composite } from '@allors/workspace/meta/system';
import { Predicate } from './predicate';
import { Sort } from './sort';

export interface Filter {
  kind: 'Filter';

  objectType: Composite;

  predicate?: Predicate;

  sorting?: Sort[];
}
