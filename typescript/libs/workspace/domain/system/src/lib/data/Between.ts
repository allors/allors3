import { RoleType } from '../meta/RoleType';
import { UnitTypes } from '../runtime/Types';
import { ParameterizablePredicate } from './ParameterizablePredicate';

export interface Between extends ParameterizablePredicate {
  roleType: RoleType;

  values?: UnitTypes[];
  
  paths?: RoleType[];
}
