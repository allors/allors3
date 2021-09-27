import { MetaPopulation } from '@allors/workspace/meta/system';
import { IObjectFactory } from './iobject-factory';

export interface IConfiguration {
  name: string;

  metaPopulation: MetaPopulation;

  objectFactory: IObjectFactory;
}
