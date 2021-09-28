import { IRange } from '../../collections/ranges/ranges';

export class Revocation {
  constructor(public version: number, public permissionIds: IRange<number>) {}
}
