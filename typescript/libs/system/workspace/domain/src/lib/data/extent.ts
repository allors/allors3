import { Filter } from './filter';
import { Union } from './union';
import { Intersect } from './intersect';
import { Except } from './except';

export type Extent = Filter | Union | Intersect | Except;

export type ExtentKind = Extent['kind'];
