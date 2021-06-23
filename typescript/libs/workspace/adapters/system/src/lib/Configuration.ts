import { MetaPopulation } from '@allors/workspace/domain/system';
import { ObjectFactory } from './ObjectFactory';

export class Configuration {
  constructor(public name: string, public metaPopulation: MetaPopulation, public objectFactory: ObjectFactory) {}
}
