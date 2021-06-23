import { ObjectType } from '../meta/ObjectType';
import { Predicate } from './Predicate';
import { Sort } from './Sort';

export interface Extent {
  objectType: ObjectType;

  predicate?: Predicate;

  sorting?: Sort[];
}
