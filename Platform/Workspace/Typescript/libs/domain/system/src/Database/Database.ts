import { ObjectType, MetaPopulation, OperandType } from '@allors/meta/system';
import { Operations, PullResponse, SyncRequest, SyncResponse, SecurityRequest, SecurityResponse } from '@allors/protocol/system';

import { DatabaseObject } from './DatabaseObject';
import { Session } from '../Session/Session';
import { Permission } from '../Permission';
import { AccessControl } from '../AccessControl';

export interface Database {
  readonly metaPopulation: MetaPopulation;
  readonly workspaceConstructorByObjectType: Map<ObjectType, any>;
  readonly sessionConstructorByObjectType: Map<ObjectType, any>;
  readonly accessControlById: Map<string, AccessControl>;

  createSession(): Session;

  diff(data: PullResponse): SyncRequest;
  sync(data: SyncResponse): SecurityRequest | undefined;
  security(data: SecurityResponse):  SecurityRequest | undefined;
  get(id: string): DatabaseObject | undefined;
  permission(objectType: ObjectType, operandType: OperandType, operation: Operations): Permission | undefined;
}
