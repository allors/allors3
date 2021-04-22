import { IComposite } from './IComposite';

export interface IInterface extends IComposite {
  directSubtypes: IComposite[];
}
