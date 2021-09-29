import { Extent } from '../../data/extent';
import { Predicate } from '../../data/predicate';
import { Result } from '../../data/result';
import { Select } from '../../data/select';
import { Sort } from '../../data/sort';
import { IObject } from '../../iobject';
import { TypeForParameter } from '../../types';

export interface FlatPull {
  extentRef?: string;

  extent?: Extent;

  predicate?: Predicate;

  sorting?: Sort[];

  object?: IObject;

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
