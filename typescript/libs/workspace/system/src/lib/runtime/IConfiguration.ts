import { MetaPopulation } from '../meta/MetaPopulation';
import { IObjectFactory } from './IObjectFactory';

export interface IConfiguration {
  name: string;

  metaPopulation: MetaPopulation;

  objectFactory: IObjectFactory;
}
