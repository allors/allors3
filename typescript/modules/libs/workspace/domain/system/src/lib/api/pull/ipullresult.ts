import { AssociationType, Class, Interface, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../../IObject';
import { IUnit } from '../../Types';
import { IResult } from '../IResult';

export interface IPullResult extends IResult {
  mergeErrors: IObject[];

  collections: Map<string, ReadonlyArray<IObject>>;

  objects: Map<string, IObject>;

  values: Map<string, IUnit>;

  collection<T extends IObject>(nameOrClass: string | Class | Interface | AssociationType | RoleType): T[];

  object<T extends IObject>(nameOrClass: string | Class | Interface): T;

  value(name: string): IUnit | Array<IUnit>;

  addMergeError(object: IObject);
}
