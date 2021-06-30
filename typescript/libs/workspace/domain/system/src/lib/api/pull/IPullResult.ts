import { IObject } from '../../runtime/IObject';
import { UnitTypes } from '../../runtime/Types';
import { IResult } from '../IResult';

export interface IPullResult extends IResult {
  collections: Map<string, ReadonlyArray<IObject>>;

  objects: Map<string, IObject>;

  values: Map<string, UnitTypes>;

  collection<T extends IObject>(name: string): T[];

  object<T extends IObject>(name: string): T;

  value(name: string): UnitTypes | Array<UnitTypes>;
}
