import { IObject, IObject } from '@allors/workspace/domain/system';
import { RoleType, ObjectType } from '@allors/workspace/meta/system';

export interface ObjectData extends Partial<IObject> {

  associationId?: string;
  associationObjectType?: ObjectType;
  associationRoleType?: RoleType;

  onCreate?: (object: IObject) => void;
}
