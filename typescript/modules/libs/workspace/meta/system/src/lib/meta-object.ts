import { Origin } from './origin';
import { MetaPopulation } from './meta-population';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface MetaObjectExtension {}

export interface MetaObject {
  readonly metaPopulation: MetaPopulation;
  tag: string;
  origin: Origin;
}
