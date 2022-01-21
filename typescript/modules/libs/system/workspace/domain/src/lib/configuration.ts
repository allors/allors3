import { MetaPopulation } from '@allors/system/workspace/meta';
import { IObjectFactory } from './iobject-factory';

export interface Configuration {
  name: string;

  metaPopulation: MetaPopulation;

  objectFactory: IObjectFactory;
}
