import { ObjectType, AssociationType, RoleType, MethodType, OperandType } from '@allors/meta/system';
import { Operations, PushRequestObject, PushRequestNewObject } from '@allors/protocol/system';

import { Composite } from '../Composite';
import { DatabaseObject } from '../Database/DatabaseObject';

import { Workspace } from './Workspace';

export interface WorkspaceObject extends Composite {
  readonly id: string;
  readonly objectType: ObjectType;
  readonly newId?: string;
  readonly version: string | undefined;

  readonly isNew: boolean;

  readonly workspace: Workspace;
  readonly databaseObject?: DatabaseObject;

  readonly hasChanges: boolean;

  canRead(roleType: RoleType): boolean | undefined;
  canWrite(roleTyp: RoleType): boolean | undefined;
  canExecute(methodType: MethodType): boolean | undefined;
  isPermited(operandType: OperandType, operation: Operations): boolean | undefined;

  get(roleType: RoleType): any;
  set(roleType: RoleType, value: any): void;
  add(roleType: RoleType, value: any): void;
  remove(roleType: RoleType, value: any): void;

  getAssociation(associationType: AssociationType): any;

  save(): PushRequestObject | undefined;
  saveNew(): PushRequestNewObject;
  reset(): void;
}
