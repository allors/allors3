import { Composite } from './composite';

export interface Interface extends Composite {
  readonly kind: 'Interface';
  subtypes: Set<Composite>;
}
