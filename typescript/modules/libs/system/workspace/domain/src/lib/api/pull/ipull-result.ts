import {
  AssociationType,
  Class,
  Interface,
  RoleType,
} from '@allors/system/workspace/meta';
import { IObject } from '../../iobject';
import { IResult } from '../iresult';

export interface IPullResult extends IResult {
  mergeErrors: IObject[];

  collections: Map<string, ReadonlyArray<IObject>>;

  objects: Map<string, IObject>;

  collection<T extends IObject>(
    nameOrClass: string | Class | Interface | AssociationType | RoleType
  ): T[];

  object<T extends IObject>(
    nameOrClass: string | Class | Interface | AssociationType | RoleType
  ): T;

  addMergeError(object: IObject);
}
