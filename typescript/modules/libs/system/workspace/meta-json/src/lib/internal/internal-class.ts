import { Class } from '@allors/system/workspace/meta';

import { Lookup } from '../utils/lookup';
import { InternalComposite } from './internal-composite';

export interface InternalClass extends InternalComposite, Class {
  deriveOverridden(lookup: Lookup): void;
}
