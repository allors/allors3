import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../runtime/IObject';
import { UnitType } from '../runtime/Types';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Equals extends ParameterizablePredicateBase {
  kind: 'Equals';
  propertyType: PropertyType;
  value?: UnitType;
  object?: IObject;
  path?: RoleType;
}
