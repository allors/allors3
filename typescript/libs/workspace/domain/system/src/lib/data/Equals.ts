import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../IObject';
import { UnitTypes } from '../Types';
import { ParameterizablePredicate } from './ParameterizablePredicate';

export interface Equals extends ParameterizablePredicate {
  propertyType: PropertyType;
  value?: UnitTypes;
  object?: IObject;
  path?: RoleType;
}
