import { RelationType } from '@allors/system/workspace/meta';
import { IObject } from './iobject';

export interface Role {
  object: IObject;

  relationType: RelationType;
}
