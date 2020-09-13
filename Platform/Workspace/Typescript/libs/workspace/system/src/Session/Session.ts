import { ObjectType } from '@allors/meta/system';
import { PushRequest, PushResponse } from '@allors/protocol/system';

import { DatabaseObject } from './DatabaseObject';
import { Database } from '../Database/Database';

export interface Session {
  readonly database: Database;

  readonly hasChanges: boolean;

  get(id: string): DatabaseObject | undefined;

  create(objectType: ObjectType | string): DatabaseObject;

  delete(object: DatabaseObject): void;

  pushRequest(): PushRequest;

  pushResponse(saveResponse: PushResponse): void;

  reset(): void;
}
