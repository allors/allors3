import { IInterface } from '@allors/workspace/system';
import { IAssociationTypeInternals } from './IAssociationTypeInternals';
import { ICompositeInternals } from './ICompositeInternals';
import { IMethodTypeInternals } from './IMethodTypeInternals';
import { IRoleTypeInternals } from './IRoleTypeInternals';

export interface IInterfaceInternals extends ICompositeInternals, IInterface {
  deriveSub(): void;

  associationTypeGenerator(): IterableIterator<IAssociationTypeInternals>;

  roleTypeGenerator(): IterableIterator<IRoleTypeInternals>;

  methodTypeGenerator(): IterableIterator<IMethodTypeInternals>;
}
