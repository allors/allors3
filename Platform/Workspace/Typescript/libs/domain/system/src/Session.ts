import { ObjectType } from '@allors/meta/system';
import { PushRequest, PushResponse } from '@allors/protocol/system';

import { SessionObject } from './SessionObject';
import { Database } from './Database';

export interface Session {
  readonly workspace: Database;

  readonly hasChanges: boolean;

  get(id: string): SessionObject | undefined;

  create(objectType: ObjectType | string): SessionObject;

  delete(object: SessionObject): void;

  pushRequest(): PushRequest;

  pushResponse(saveResponse: PushResponse): void;

  reset(): void;
}
