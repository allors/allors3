import { RoleType } from '@allors/workspace/meta/system';
import { UnitTypes } from '../runtime/Types';
import { ParameterizablePredicate } from './ParameterizablePredicate';

export interface Between extends ParameterizablePredicate {
  roleType: RoleType;

  values?: UnitTypes[];
  
  paths?: RoleType[];
}
