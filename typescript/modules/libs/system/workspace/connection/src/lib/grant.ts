import { IRange } from './collections/ranges/ranges';

/**
 * A grant of the user: the permissions it holds, at the version the server sent.
 */
export class Grant {
  constructor(
    public readonly id: number,
    public readonly version: number,
    public readonly permissionIds: IRange<number>
  ) {}
}
