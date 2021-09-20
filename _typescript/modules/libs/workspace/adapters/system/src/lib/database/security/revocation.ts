import { IRange } from "../../collections/ranges/Ranges";

export class Revocation {
  constructor(public version: number, public permissionIds: IRange<number>) {}
}
