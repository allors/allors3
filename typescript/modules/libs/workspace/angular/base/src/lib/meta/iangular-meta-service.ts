import { Composite, RoleType } from '@allors/workspace/meta/system';
import { IAngularComposite } from './iangular-composite';
import { IAngularRoleType } from './iangular-role-type';

export type IAngularMetaObject = IAngularComposite | IAngularRoleType;

export interface IAngularMetaService {
  for(composite: Composite): IAngularComposite;

  for(roleType: RoleType): IAngularRoleType;
}
