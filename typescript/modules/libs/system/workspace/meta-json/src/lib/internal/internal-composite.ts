import { AssociationType, Composite, ObjectType, RoleType } from '@allors/workspace/meta/system';

import { Lookup } from '../utils/lookup';
import { InternalInterface } from './internal-interface';

export interface InternalComposite extends ObjectType, Composite {
  derive(lookup: Lookup): void;
  deriveSuper(): void;
  deriveOperand(): void;
  deriveOriginRoleType(): void;
  derivePropertyTypeByPropertyName(): void;
  deriveDependencies(): void;
  supertypeGenerator(): IterableIterator<InternalInterface>;
  onNewAssociationType(associationType: AssociationType): void;
  onNewRoleType(roleType: RoleType): void;
}
