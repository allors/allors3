import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../IObject';
import { IUnit } from '../Types';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Equals extends ParameterizablePredicateBase {
  kind: 'Equals';
  propertyType?: PropertyType;
  value?: IUnit;
  object?: IObject;
  path?: RoleType;
}
