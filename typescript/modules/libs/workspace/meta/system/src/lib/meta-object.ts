import { Origin } from './origin';
import { MetaPopulation } from './meta-population';

export interface MetaObject {
  metaPopulation: MetaPopulation;

  tag: string;

  origin: Origin;
}
