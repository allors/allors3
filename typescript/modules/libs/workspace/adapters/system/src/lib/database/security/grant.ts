import { IRange } from "../../collections/ranges/Ranges";

export class Grant {
  constructor(public version: number, public permissionIds: IRange<number>) {}
}
