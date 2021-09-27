import { MetaPopulation } from '@allors/workspace/meta/system';
import { PrototypeObjectFactory } from './prototype-object-factory';

export class Configuration {
  constructor(public name: string, public metaPopulation: MetaPopulation, public objectFactory: PrototypeObjectFactory) {}
}
