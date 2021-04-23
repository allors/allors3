import { IComposite } from './IComposite';

export interface IInterface extends IComposite {
  subtypes: Readonly<Set<IComposite>>;
}
