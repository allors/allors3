import { ReadInput, ReadSuccess } from '@allors/system/common/protocol-json';
import { ReadStoreContents } from './read-store';
import {
  ReadStoredPermission,
  ReadStoredRecord,
  ReadStoredSecurity,
  ReadStoreScope,
} from './read-store-values';

/** Immutable values held directly by the request, result and Session baselines. */
export class CompletedRead {
  readonly scope: ReadStoreScope;
  readonly records: readonly ReadStoredRecord[];
  readonly security: readonly ReadStoredSecurity[];
  readonly permissions: readonly ReadStoredPermission[];
  readonly outcomes: readonly Readonly<ReadInput>[];
  readonly collections: Readonly<Record<string, readonly number[]>>;
  readonly objects: Readonly<Record<string, number | null>>;

  constructor(
    scope: ReadStoreScope,
    contents: ReadStoreContents,
    response: ReadSuccess
  ) {
    this.scope = scope;
    this.records = Object.freeze([...(contents.records ?? [])]);
    this.security = Object.freeze([...(contents.security ?? [])]);
    this.permissions = Object.freeze([...(contents.permissions ?? [])]);
    this.outcomes = Object.freeze(
      response.l.map((value) => Object.freeze({ ...value }))
    );
    const collections = Object.create(null) as Record<
      string,
      readonly number[]
    >;
    for (const [key, value] of Object.entries(response.c))
      collections[key] = Object.freeze([...value]);
    this.collections = Object.freeze(collections);
    this.objects = Object.freeze({ ...response.o });
    Object.freeze(this);
  }
}
