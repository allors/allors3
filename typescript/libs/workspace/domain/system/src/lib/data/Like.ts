import { RoleType } from '../meta/RoleType';
import { ParameterizablePredicate } from './ParameterizablePredicate';

export interface Like extends ParameterizablePredicate {
  roleType: RoleType;
  value?: string;
}
