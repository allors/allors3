import { RelationType } from '@allors/workspace/meta/system';
import { IObject } from '../iobject';

export interface IDiff {
  relationType: RelationType;

  assocation: IObject;
}
