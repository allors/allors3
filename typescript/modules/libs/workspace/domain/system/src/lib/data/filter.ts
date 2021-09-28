import { ObjectType } from '@allors/workspace/meta/system';
import { Predicate } from './predicate';
import { Sort } from './sort';

export interface Filter {
  kind: 'Filter';

  objectType: ObjectType;

  predicate?: Predicate;

  sorting?: Sort[];
}
