import { Origin } from '../Origin';
import { MetaPopulation } from './MetaPopulation';

export interface MetaObject {
  metaPopulation: MetaPopulation;

  tag: number;

  origin: Origin;
}
