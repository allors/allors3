import { IComposite } from '@allors/workspace/system';
import { IAssociationTypeInternals } from './IAssociationTypeInternals';
import { IInterfaceInternals } from './IInterfaceInternals';
import { IObjectTypeInternals } from './IObjectTypeInternals';
import { IRoleTypeInternals } from './IRoleTypeInternals';

export interface ICompositeInternals extends IObjectTypeInternals, IComposite {
  derive(): void;
  deriveSuper(): void;
  supertypeGenerator(): IterableIterator<IInterfaceInternals>;
  onNewAssociationType(associationType: IAssociationTypeInternals): void;
  onNewRoleType(roleType: IRoleTypeInternals): void;
}
