import { MetaPopulation } from '@allors/workspace/meta/system';
import { InternalMetaObject } from './internal-meta-object';
import { InternalObjectType } from './internal-object-type';
import { InternalComposite } from './internal-composite';

export interface InternalMetaPopulation extends MetaPopulation {
  onNew(metaObject: InternalMetaObject): void;
  onNewObjectType(objectType: InternalObjectType): void;
  onNewComposite(objectType: InternalComposite): void;
}
