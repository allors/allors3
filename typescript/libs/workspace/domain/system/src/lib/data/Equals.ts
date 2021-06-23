import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../runtime/IObject';
import { UnitTypes } from '../runtime/Types';
import { ParameterizablePredicate } from './ParameterizablePredicate';

export interface Equals extends ParameterizablePredicate {
  propertyType: PropertyType;
  value?: UnitTypes;
  object?: IObject;
  path?: RoleType;
}
