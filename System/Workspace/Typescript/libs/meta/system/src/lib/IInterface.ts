import { IComposite } from "./IComposite";

export interface IInterface extends IComposite {
  DirectSubtypes: IComposite;

  Subtypes: IComposite;
}
