import { ObjectType } from '@allors/workspace/system';
import { PushRequest, PushResponse } from '@allors/protocol/json/system';

import { DatabaseObject } from './DatabaseObject';
import { Database } from '../Remote/Remote';
import { WorkspaceObject } from './WorkspaceObject';

export interface Session {
  readonly database: Database;

  readonly hasChanges: boolean;

  get(id: string): DatabaseObject | WorkspaceObject | undefined;

  create(objectType: ObjectType | string): DatabaseObject | WorkspaceObject;

  delete(object: DatabaseObject | WorkspaceObject): void;

  pushRequest(): PushRequest;

  pushResponse(saveResponse: PushResponse): void;

  reset(): void;
}
