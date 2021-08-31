import { IRange } from "../../collections/ranges/Ranges";

export class AccessControl {
  constructor(public version: number, public permissionIds: IRange<number>) {}
}
