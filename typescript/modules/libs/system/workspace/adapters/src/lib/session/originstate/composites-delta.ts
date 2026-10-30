import { IObject } from '@allors/system/workspace/domain';
import { IRange } from '../../collections/ranges/ranges';

export interface CompositesDelta {
  added: IRange<IObject>;
  removed: IRange<number>;
}
