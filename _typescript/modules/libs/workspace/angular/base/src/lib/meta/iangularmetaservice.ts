import { Composite, RoleType } from '@allors/workspace/meta/system';
import { IAngularComposite } from './IAngularComposite';
import { IAngularRoleType } from './IAngularRoleType';

export type IAngularMetaObject = IAngularComposite | IAngularRoleType;

export interface IAngularMetaService {
  for(composite: Composite): IAngularComposite;

  for(roleType: RoleType): IAngularRoleType;
}
