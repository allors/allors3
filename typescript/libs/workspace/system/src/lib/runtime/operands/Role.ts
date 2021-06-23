import { RelationType } from "../../meta/RelationType";
import { IObject } from "../IObject";

export interface Role {
  object: IObject;

  relationType: RelationType;
}
