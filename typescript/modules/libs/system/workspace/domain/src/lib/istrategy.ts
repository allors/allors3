import {
  AssociationType,
  Class,
  MethodType,
  RoleType,
} from '@allors/system/workspace/meta';
import { IDiff } from './diff/idiff';
import { IObject } from './iobject';
import { ISession } from './isession';
import { IUnit } from './types';

export interface IStrategy {
  session: ISession;

  object: IObject;

  cls: Class;

  id: number;

  version: number;

  isNew: boolean;

  hasChanges: boolean;

  isDeleted: boolean;

  delete(): void;

  reset(): void;

  diff(): IDiff[];

  canRead(roleType: RoleType): boolean;

  canWrite(roleType: RoleType): boolean;

  canExecute(methodType: MethodType): boolean;

  existRole(roleType: RoleType): boolean;

  hasChanged(roleType: RoleType): boolean;

  restoreRole(assignedRoleType: RoleType): void;

  getRole(roleType: RoleType): unknown;

  getUnitRole(roleType: RoleType): IUnit;

  getCompositeRole<T extends IObject>(
    roleType: RoleType,
    skipMissing?: boolean
  ): T;

  getCompositesRole<T extends IObject>(
    roleType: RoleType,
    skipMissing?: boolean
  ): ReadonlyArray<T>;

  setRole(roleType: RoleType, value: unknown): void;

  setUnitRole(roleType: RoleType, value: IUnit): void;

  setCompositeRole<T extends IObject>(roleType: RoleType, value: T): void;

  setCompositesRole<T extends IObject>(
    roleType: RoleType,
    value: ReadonlyArray<T>
  ): void;

  addCompositesRole<T extends IObject>(roleType: RoleType, value: T): void;

  removeCompositesRole<T extends IObject>(roleType: RoleType, value: T): void;

  removeRole(roleType: RoleType): void;

  getCompositeAssociation<T extends IObject>(
    associationType: AssociationType
  ): T;

  getCompositesAssociation<T extends IObject>(
    associationType: AssociationType
  ): ReadonlyArray<T>;
}
