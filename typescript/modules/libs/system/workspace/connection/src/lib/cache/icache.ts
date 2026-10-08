import { Class, MetaPopulation, OperandType } from '@allors/system/workspace/meta';
import { Grant } from '../grant';
import { Operations } from '../operations';
import { Permission } from '../permission';
import { IRecord } from '../record';
import { RecordChangedEvent } from '../record-changed-event';
import { Revocation } from '../revocation';
import { Subscribable } from '../subscribable';

/**
 * What a connection keeps of the user's view of the database: records, grants, revocations
 * and permissions, by id. A connection that is given no cache creates a MemoryCache of its
 * own.
 */
export interface ICache {
  /**
   * Raised when a record the cache held is replaced, at the moment it is: the grants,
   * revocations and permissions the new record names may not be in yet. A connection raises
   * its own recordChanged once they are.
   */
  readonly recordChanged: Subscribable<RecordChangedEvent>;

  /**
   * The name of the workspace whose records the cache holds.
   */
  readonly workspaceName: string;

  /**
   * The meta population the records are typed by.
   */
  readonly metaPopulation: MetaPopulation;

  getRecord(id: number): IRecord | undefined;

  /**
   * Keeps the record, and answers whether it was kept.
   */
  setRecord(record: IRecord): boolean;

  /**
   * Forgets the record of the object, if the cache holds one.
   */
  removeRecord(id: number): void;

  /**
   * Forgets everything.
   */
  clear(): void;

  getGrant(id: number): Grant | undefined;

  setGrant(grant: Grant): void;

  getRevocation(id: number): Revocation | undefined;

  setRevocation(revocation: Revocation): void;

  hasPermission(id: number): boolean;

  setPermission(permission: Permission): void;

  /**
   * The id of the permission for the operation on the operand type of the class, or 0 when
   * the cache holds no such permission.
   */
  getPermission(
    cls: Class,
    operandType: OperandType,
    operation: Operations
  ): number;
}
