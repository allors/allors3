import { IComposite } from './IComposite';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface IInterface extends IComposite {
  subtypes: IComposite[];
}
