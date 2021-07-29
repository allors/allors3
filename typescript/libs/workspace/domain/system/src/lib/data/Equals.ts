import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../runtime/IObject';
import { IUnit } from '../runtime/Types';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Equals extends ParameterizablePredicateBase {
  kind: 'Equals';
  propertyType: PropertyType;
  value?: IUnit;
  object?: IObject;
  path?: RoleType;
}
