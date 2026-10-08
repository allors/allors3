import { IIdentifiable, TypeForParameter } from '../types';
import { Node } from '../pointer/node';
import { Extent } from './extent';
import { Predicate } from './predicate';
import { Result } from './result';
import { Select } from './select';
import { Sort } from './sort';

export interface FlatPull {
  extentRef?: string;

  extent?: Extent;

  predicate?: Predicate;

  sorting?: Sort[];

  object?: IIdentifiable;

  objectId?: number | string;

  selectRef?: string;

  select?: Select | any;

  include?: Node[] | any;

  name?: string;

  skip?: number;

  take?: number;

  results?: Result[] | any;

  arguments?: { [name: string]: TypeForParameter };
}
