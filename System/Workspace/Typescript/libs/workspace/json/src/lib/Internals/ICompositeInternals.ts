import { IComposite, IMetaPopulation, IObjectType } from '@allors/workspace/system';

export interface ICompositeInternals extends IComposite {
  init(): void;
}
