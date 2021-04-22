import { Origin } from '../Origin';
import { IMetaPopulation } from './IMetaPopulation';

export interface IMetaObject {
  metaPopulation: IMetaPopulation;

  tag: number;

  origin: Origin;
}
