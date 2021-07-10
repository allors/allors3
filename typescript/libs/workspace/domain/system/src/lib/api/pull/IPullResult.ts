import { IObject } from '../../runtime/IObject';
import { UnitType } from '../../runtime/Types';
import { IResult } from '../IResult';

export interface IPullResult extends IResult {
  collections: Map<string, ReadonlyArray<IObject>>;

  objects: Map<string, IObject>;

  values: Map<string, UnitType>;

  collection<T extends IObject>(name: string): T[];

  object<T extends IObject>(name: string): T;

  value(name: string): UnitType | Array<UnitType>;
}
