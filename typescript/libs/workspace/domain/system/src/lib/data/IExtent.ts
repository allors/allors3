import { Filter } from './Filter';
import { Union } from './Union';
import { Intersect } from './Intersect';
import { Except } from './Except';

export type IExtent = Filter | Union | Intersect | Except;
