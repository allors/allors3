import { Dependency } from '@allors/workspace/meta/system';
import { IObject } from '../../iobject';
import { ICycle } from './icycle';
import { IPattern } from './ipattern';

export interface IRule {
  patterns: IPattern[];

  dependencies: Dependency[];

  derive(cycle: ICycle, matches: IObject[]): void;
}
