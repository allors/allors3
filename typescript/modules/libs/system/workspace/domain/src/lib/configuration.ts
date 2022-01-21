import { MetaPopulation } from '@allors/workspace/meta/system';
import { IObjectFactory } from './iobject-factory';

export interface Configuration {
  name: string;

  metaPopulation: MetaPopulation;

  objectFactory: IObjectFactory;
}
