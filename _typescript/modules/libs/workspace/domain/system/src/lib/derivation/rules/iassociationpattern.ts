import { AssociationType } from '@allors/workspace/meta/system';
import { IPatternBase } from './IPattern';

export interface IAssociationPattern extends IPatternBase {
  kind: 'AssociationPattern';

  AssociationType: AssociationType;
}
