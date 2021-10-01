import { MetaObject, MetaPopulation, ObjectType } from '@allors/workspace/meta/system';
import { InternalComposite } from './internal-composite';

export interface InternalMetaPopulation extends MetaPopulation {
  onNew(metaObject: MetaObject): void;
  onNewObjectType(objectType: ObjectType): void;
  onNewComposite(objectType: InternalComposite): void;
}
