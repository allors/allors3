import { Class } from '@allors/workspace/meta/system';
import { Lookup } from '../utils/Lookup';
import { InternalComposite } from './InternalComposite';

export interface InternalClass extends InternalComposite, Class {
  deriveOverridden(lookup: Lookup): void;
}
