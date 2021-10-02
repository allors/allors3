import { MetaPopulation } from '@allors/workspace/meta/system';
import { IdGenerator } from './database/database-connection';
import { Engine } from './session/derivation/engine';
import { PrototypeObjectFactory } from './prototype-object-factory';

export interface Configuration {
  name: string;
  metaPopulation: MetaPopulation;
  objectFactory: PrototypeObjectFactory;
  idGenerator: IdGenerator;
  engine: Engine;
  maxCycles?: number;
}
