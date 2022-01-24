import { RelationType } from '@allors/system/workspace/meta';
import { IObject } from '../iobject';

export interface IDiff {
  relationType: RelationType;

  assocation: IObject;
}
