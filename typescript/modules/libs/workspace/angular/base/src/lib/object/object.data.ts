import { IObject } from '@allors/workspace/domain/system';
import { RoleType, Composite } from '@allors/workspace/meta/system';

export interface ObjectData extends Partial<IObject> {
  associationId?: number;
  associationObjectType?: Composite;
  associationRoleType?: RoleType;

  onCreate?: (object: IObject) => void;
}
