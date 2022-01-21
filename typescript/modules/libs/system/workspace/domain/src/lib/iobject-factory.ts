import { IObject } from './iobject';
import { IStrategy } from './istrategy';

export interface IObjectFactory {
  create(strategy: IStrategy): IObject;
}
