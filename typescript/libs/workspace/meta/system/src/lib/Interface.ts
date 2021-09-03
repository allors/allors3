import { Composite } from './Composite';

export interface Interface extends Composite {
  readonly kind: 'Interface';
  subtypes: Set<Composite>;
}
