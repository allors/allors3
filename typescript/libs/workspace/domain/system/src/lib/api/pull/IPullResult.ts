import { IObject } from '../../runtime/IObject';
import { IUnit } from '../../runtime/Types';
import { IResult } from '../IResult';

export interface IPullResult extends IResult {
  mergeErrors: IObject[];

  collections: Map<string, ReadonlyArray<IObject>>;

  objects: Map<string, IObject>;

  values: Map<string, IUnit>;

  collection<T extends IObject>(name: string): T[];

  object<T extends IObject>(name: string): T;

  value(name: string): IUnit | Array<IUnit>;

  addMergeError(object: IObject);
}
