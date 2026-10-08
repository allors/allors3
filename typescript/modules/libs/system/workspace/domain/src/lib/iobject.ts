import { IIdentifiable } from '@allors/system/workspace/connection';
import { IStrategy } from './istrategy';

export interface IObject extends IIdentifiable {
  id: number;

  strategy: IStrategy;
}
