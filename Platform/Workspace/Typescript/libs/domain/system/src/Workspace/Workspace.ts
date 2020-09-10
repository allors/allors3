import { ObjectType } from '@allors/meta/system';
import { PushRequest, PushResponse } from '@allors/protocol/system';

import { WorkspaceObject } from './WorkspaceObject';
import { Database } from '../Database/Database';

export interface Workspace {
  readonly database: Database;

  readonly hasChanges: boolean;

  get(id: string): WorkspaceObject | undefined;

  create(objectType: ObjectType | string): WorkspaceObject;

  delete(object: WorkspaceObject): void;

  pushRequest(): PushRequest;

  pushResponse(saveResponse: PushResponse): void;

  reset(): void;
}
