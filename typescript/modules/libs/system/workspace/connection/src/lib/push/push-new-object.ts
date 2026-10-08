import { Class } from '@allors/system/workspace/meta';
import { RoleChange } from './role-change';

/**
 * A new object to push: its workspace id, its class and the roles it was given.
 */
export interface PushNewObject {
  workspaceId: number;

  cls: Class;

  roles?: RoleChange[] | null;
}
