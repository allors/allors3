import { MetaPopulation } from '@allors/workspace/meta/system';
import { IRule } from './derivation/irule';
import { IObject } from './iobject';
import { IObjectFactory } from './iobject-factory';

export interface Configuration {
  name: string;

  metaPopulation: MetaPopulation;

  objectFactory: IObjectFactory;

  rules: IRule<IObject>[];
}
