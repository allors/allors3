import { Class } from '@allors/workspace/meta/system';

import { Lookup } from '../utils/lookup';
import { InternalComposite } from './internal-composite';

export interface InternalClass extends InternalComposite, Class {
  deriveOverridden(lookup: Lookup): void;
}
