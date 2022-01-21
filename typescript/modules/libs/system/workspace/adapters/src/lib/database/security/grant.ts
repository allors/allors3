import { IRange } from '../../collections/ranges/ranges';

export class Grant {
  constructor(public version: number, public permissionIds: IRange<number>) {}
}
