import { ObjectType } from '@allors/workspace/meta/system';
import { ParameterTypes } from "../Types";
import { IExtent } from "./IExtent";
import { Result } from "./Result";
import { IObject } from '../IObject';

export interface Pull {
   extentRef?: string;

  extent?: IExtent;

  objectType?: ObjectType;

  object?: IObject;

  objectId?: number;

  results?: Result[];

  arguments?: { [name: string]: ParameterTypes };
}
