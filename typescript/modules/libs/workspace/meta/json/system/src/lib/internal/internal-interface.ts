import { Interface } from '@allors/workspace/meta/system';
import { InternalAssociationType } from './internal-association-type';
import { InternalComposite } from './internal-composite';
import { InternalMethodType } from './internal-method-type';
import { InternalRoleType } from './internal-role-type';

export interface InternalInterface extends InternalComposite, Interface {
  deriveSub(): void;

  associationTypeGenerator(): IterableIterator<InternalAssociationType>;

  roleTypeGenerator(): IterableIterator<InternalRoleType>;

  methodTypeGenerator(): IterableIterator<InternalMethodType>;
}
