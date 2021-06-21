import { IVisitable } from './visitor/IVisitable';

export interface Predicate extends IVisitable {
  dependencies?: string[];
}
