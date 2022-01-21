import { RelationType } from '@allors/workspace/meta/system';
import { IObject } from './iobject';

export interface Role {
  object: IObject;

  relationType: RelationType;
}
