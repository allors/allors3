import { IRange } from './collections/ranges/ranges';

/**
 * A revocation on the user: the permissions it denies, at the version the server sent.
 */
export class Revocation {
  constructor(
    public readonly id: number,
    public readonly version: number,
    public readonly permissionIds: IRange<number>
  ) {}
}
