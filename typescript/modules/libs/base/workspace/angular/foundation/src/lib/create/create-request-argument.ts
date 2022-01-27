import { IObject, IUnit } from '@allors/system/workspace/domain';
import { AssociationType, RoleType } from '@allors/system/workspace/meta';

export interface CreateRequestAssociationArgument {
  readonly kind: 'AssociationArgument';
  associationType: AssociationType;
  association?: number | number[];
}

export interface CreateRequestRoleArgument {
  readonly kind: 'RoleArgument';
  roleType: RoleType;
  role: IUnit | number | number[];
}

export interface CreateRequestCallbackArgument {
  readonly kind: 'CallbackArgument';
  callback: (object: IObject) => void;
}

export type CreateRequestArgument =
  | CreateRequestAssociationArgument
  | CreateRequestRoleArgument
  | CreateRequestCallbackArgument;
