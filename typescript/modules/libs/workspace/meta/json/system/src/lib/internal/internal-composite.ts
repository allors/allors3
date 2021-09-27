import { Composite } from '@allors/workspace/meta/system';

import { Lookup } from '../utils/lookup';
import { InternalAssociationType } from './internal-association-type';
import { InternalInterface } from './internal-interface';
import { InternalObjectType } from './internal-object-type';
import { InternalRoleType } from './internal-role-type';

export interface InternalComposite extends InternalObjectType, Composite {
  derive(lookup: Lookup): void;
  deriveSuper(): void;
  deriveOperand(): void;
  deriveOriginRoleType(): void;
  derivePropertyTypeByPropertyName(): void;
  supertypeGenerator(): IterableIterator<InternalInterface>;
  onNewAssociationType(associationType: InternalAssociationType): void;
  onNewRoleType(roleType: InternalRoleType): void;
}
