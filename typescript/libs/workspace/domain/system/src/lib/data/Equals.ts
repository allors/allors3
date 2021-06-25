import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../runtime/IObject';
import { UnitTypes } from '../runtime/Types';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Equals extends ParameterizablePredicateBase {
  kind: 'Equals';
  propertyType: PropertyType;
  value?: UnitTypes;
  object?: IObject;
  path?: RoleType;
}
