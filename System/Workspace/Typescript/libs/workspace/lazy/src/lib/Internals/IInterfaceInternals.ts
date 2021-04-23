import { IInterface } from '@allors/workspace/system';
import { ICompositeInternals } from './ICompositeInternals';

export interface IInterfaceInternals extends ICompositeInternals, IInterface {
  deriveSub(): void;
}
