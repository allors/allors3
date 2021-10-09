import { AssociationType } from '@allors/workspace/meta/system';
import { IPatternBase } from './ipattern';

export interface IAssociationPattern extends IPatternBase {
  kind: 'AssociationPattern';

  associationType: AssociationType;
}
