import { IStrategy } from './istrategy';

export interface IObject {
  id: number;

  strategy: IStrategy;
}
