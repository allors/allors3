import { ObjectType, MetaPopulation, OperandType } from '@allors/workspace/system';
import { Operations, PullResponse, SyncRequest, SyncResponse, SecurityRequest, SecurityResponse } from '@allors/protocol/json/system';

import { Session } from '../Working/Working';
import { Permission } from '../Permissions/Permission';
import { AccessControl } from '../AccessControl';

import { Record } from './RemoteObject';

export interface Database {
  readonly metaPopulation: MetaPopulation;
  readonly accessControlById: Map<string, AccessControl>;
  readonly constructorByObjectType: Map<ObjectType, any>;

  createSession(): Session;
  get(id: string): Record;
  permission(objectType: ObjectType, operandType: OperandType, operation: Operations): Permission | undefined;

  diff(data: PullResponse): SyncRequest;
  sync(data: SyncResponse): SecurityRequest | undefined;
  security(data: SecurityResponse): SecurityRequest | undefined;
}
