import { ObjectType, MetaPopulation, OperandType } from '@allors/meta/system';
import { Operations, PullResponse, SyncRequest, SyncResponse, SecurityRequest, SecurityResponse } from '@allors/protocol/system';

import { Session } from '../Session/Session';
import { Permission } from '../Permission';
import { AccessControl } from '../AccessControl';

import { DatabaseObject } from './DatabaseObject';

export interface Database {
  readonly metaPopulation: MetaPopulation;
  readonly accessControlById: Map<string, AccessControl>;

  createSession(): Session;
  get(id: string): DatabaseObject;
  permission(objectType: ObjectType, operandType: OperandType, operation: Operations): Permission | undefined;

  diff(data: PullResponse): SyncRequest;
  sync(data: SyncResponse): SecurityRequest | undefined;
  security(data: SecurityResponse): SecurityRequest | undefined;
}
