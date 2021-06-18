import { MetaPopulation } from '@allors/workspace/meta/system';
import { IObjectFactory } from './IObjectFactory';

export interface IConfiguration {
  name: string;

  metaPopulation: MetaPopulation;

  objectFactory: IObjectFactory;
}
