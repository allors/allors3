import { DatabaseObject, ParameterTypes } from '@allors/workspace/core';

import { IExtent } from './IExtent';
import { Result } from './Result';
import { Select } from './Select';
import { Predicate } from './Predicate';
import { Sort } from './Sort';
import { Tree } from './Tree';

export interface FlatPull {
  extentRef?: string;

  extent?: IExtent;

  predicate?: Predicate;

  sort?: Sort | Sort[];

  object?: DatabaseObject | string;

  results?: Result[];

  selectRef?: string;

  select?: Select | any;

  include?: Tree | any;

  parameters?: { [id: string]: ParameterTypes };

  name?: string;

  skip?: number;

  take?: number;
}
