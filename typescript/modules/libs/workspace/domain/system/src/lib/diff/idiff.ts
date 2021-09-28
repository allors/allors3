import { RelationType } from '@allors/workspace/meta/system';
import { IStrategy } from '../istrategy';

export interface IDiff {
  relationType: RelationType;

  assocation: IStrategy;
}
