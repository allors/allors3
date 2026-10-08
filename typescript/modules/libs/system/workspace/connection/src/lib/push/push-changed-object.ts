import { RoleChange } from './role-change';

/**
 * A changed database object to push: its id, the version the changes were made from, which
 * the server compares with its own, and the changed roles.
 */
export interface PushChangedObject {
  id: number;

  version: number;

  roles?: RoleChange[] | null;
}
