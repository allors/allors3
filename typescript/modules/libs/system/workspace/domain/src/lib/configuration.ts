import { MetaPopulation } from '@allors/system/workspace/meta';
import { IRule } from './derivation/irule';
import { IObject } from './iobject';
import { IObjectFactory } from './iobject-factory';

export interface Configuration {
  name: string;

  metaPopulation: MetaPopulation;

  objectFactory: IObjectFactory;

  rules?: IRule<IObject>[];
}
