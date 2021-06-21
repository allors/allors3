import { IVisitor } from "./IVisitor";

export interface IVisitable {
  accept(visitor: IVisitor): void;
}
