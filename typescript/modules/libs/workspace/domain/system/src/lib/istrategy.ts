import { AssociationType, Class, MethodType, RoleType } from '@allors/workspace/meta/system';
import { IDiff } from './diff/IDiff';
import { IObject } from './IObject';
import { ISession } from './ISession';
import { IUnit } from './Types';

export interface IStrategy {
  session: ISession;

  object: IObject;

  cls: Class;

  id: number;

  version: number;

  isNew: boolean;

  reset(): void;

  diff(): IDiff[];

  canRead(roleType: RoleType): boolean;

  canWrite(roleType: RoleType): boolean;

  canExecute(methodType: MethodType): boolean;

  existRole(roleType: RoleType): boolean;

  hasChangedRole(roleType: RoleType): boolean;

  restoreRole(assignedRoleType: RoleType): void;

  getRole(roleType: RoleType): unknown;

  getUnitRole(roleType: RoleType): IUnit;

  getCompositeRole<T extends IObject>(roleType: RoleType): T;

  getCompositesRole<T extends IObject>(roleType: RoleType): ReadonlyArray<T>;

  setRole(roleType: RoleType, value: unknown): void;

  setUnitRole(roleType: RoleType, value: IUnit): void;

  setCompositeRole<T extends IObject>(roleType: RoleType, value: T): void;

  setCompositesRole<T extends IObject>(roleType: RoleType, value: ReadonlyArray<T>): void;

  addCompositesRole<T extends IObject>(roleType: RoleType, value: T): void;

  removeCompositesRole<T extends IObject>(roleType: RoleType, value: T): void;

  removeRole(roleType: RoleType): void;

  getCompositeAssociation<T extends IObject>(associationType: AssociationType): T;

  getCompositesAssociation<T extends IObject>(associationType: AssociationType): ReadonlyArray<T>;
}