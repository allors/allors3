import { MethodType } from "../../meta/MethodType";
import { IObject } from "../IObject";

export interface Method {
  object: IObject;

  MethodType: MethodType;
}
