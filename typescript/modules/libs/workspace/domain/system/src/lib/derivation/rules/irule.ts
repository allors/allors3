import { IObject } from '../../iobject';
import { Dependency } from '../dependencies/dependency';
import { ICycle } from './icycle';
import { IPattern } from './ipattern';

export interface IRule {
  patterns: IPattern[];

  dependencies: Dependency[];

  derive(cycle: ICycle, matches: IObject[]): void;
}
