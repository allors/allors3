import { AssociationType, Class, MethodType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from './IObject';
import { ISession } from './ISession';
import { UnitTypes } from './Types';

export interface IStrategy {
  object: IObject;

  cls: Class;

  id: number;

  session: ISession;

  canRead(roleType: RoleType): boolean;

  canWrite(roleType: RoleType): boolean;

  canExecute(methodType: MethodType): boolean;

  exist(roleType: RoleType): boolean;

  get(roleType: RoleType): unknown;

  getUnit(roleType: RoleType): UnitTypes;

  getComposite<T extends IObject>(roleType: RoleType): T;

  getComposites<T extends IObject>(roleType: RoleType): T[];

  set(roleType: RoleType, value: unknown): void;

  setUnit(roleType: RoleType, value: UnitTypes): void;

  setComposite<T extends IObject>(roleType: RoleType, value: T): void;

  setComposites<T extends IObject>(roleType: RoleType, value: T[]): void;

  add<T extends IObject>(roleType: RoleType, value: T): void;

  remove<T extends IObject>(roleType: RoleType, value: T): void;

  removeAll(roleType: RoleType): void;

  getCompositeAssociation<T extends IObject>(associationType: AssociationType): T;

  getCompositesAssociation<T extends IObject>(associationType: AssociationType): T[];
}
