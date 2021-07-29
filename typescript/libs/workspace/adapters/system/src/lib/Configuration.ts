import { MetaPopulation } from '@allors/workspace/meta/system';
import { PrototypeObjectFactory } from './PrototypeObjectFactory';

export class Configuration {
  constructor(public name: string, public metaPopulation: MetaPopulation, public objectFactory: PrototypeObjectFactory) {}
}
