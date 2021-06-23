import { ObjectType } from "@allors/workspace/meta/system";
import { IExtent } from "../../data/IExtent";
import { Result } from "../../data/Result";
import { IObject } from "../../runtime/IObject";
import { ParameterTypes } from "../../runtime/Types";

export interface Pull {
   extentRef?: string;

  extent?: IExtent;

  objectType?: ObjectType;

  object?: IObject;

  objectId?: number;

  results?: Result[];

  arguments?: { [name: string]: ParameterTypes };
}
