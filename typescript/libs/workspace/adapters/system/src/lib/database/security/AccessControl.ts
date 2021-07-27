import { IRange } from "../../collections/Range";

export class AccessControl {
  constructor(public version: number, public permissionIds: IRange) {}
}
