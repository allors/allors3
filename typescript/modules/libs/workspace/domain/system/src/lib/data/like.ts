import { RoleType } from '@allors/workspace/meta/system';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Like extends ParameterizablePredicateBase {
  kind: 'Like';
  roleType: RoleType;
  value?: string;
}
