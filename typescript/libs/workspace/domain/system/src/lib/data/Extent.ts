import { ObjectType } from '@allors/workspace/meta/system';
import { Predicate } from './Predicate';
import { Sort } from './Sort';
import { IVisitable } from './visitor/IVisitable';

export interface Extent extends IVisitable {
  objectType: ObjectType;

  predicate?: Predicate;

  sorting?: Sort[];
}
