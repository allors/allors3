import { ObjectType, AssociationType, RoleType } from '@allors/workspace/system';

import { Session } from './Working';

export interface WorkspaceObject {
  readonly id: string;
  readonly objectType: ObjectType;

  readonly session: Session;

  get(roleType: RoleType): any;
  set(roleType: RoleType, value: any): void;
  add(roleType: RoleType, value: any): void;
  remove(roleType: RoleType, value: any): void;

  getAssociation(associationType: AssociationType): any;
}
