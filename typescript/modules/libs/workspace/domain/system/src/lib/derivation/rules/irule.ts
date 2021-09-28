import { IObject } from '../../iobject';
import { ICycle } from './icycle';
import { IPattern } from './ipattern';

export interface IRule {
  id: string;

  patterns: IPattern[];

  derive(cycle: ICycle, matches: IObject[]): void;
}
