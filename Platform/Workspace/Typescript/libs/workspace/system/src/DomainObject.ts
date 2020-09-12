import { ObjectType } from '@allors/meta/system';

export interface DomainObject {
  readonly id: string;
  readonly objectType: ObjectType;

  readonly version: string | undefined;
}
