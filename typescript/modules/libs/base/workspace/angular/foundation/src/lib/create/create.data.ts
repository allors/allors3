import { Composite, RoleType } from '@allors/system/workspace/meta';
import { OnCreate } from './create.service';

export interface CreateData {
  readonly kind: 'CreateData';
  objectType: Composite;
  args?: [{ roleType: RoleType; objectId?: number; objectIds?: number[] }];
  onCreate?: OnCreate;
}
