import { Origin } from '@allors/shared/system';
import { MetaPopulation } from './MetaPopulation';

export interface MetaObject {
  metaPopulation: MetaPopulation;

  tag: number;

  origin: Origin;
}
