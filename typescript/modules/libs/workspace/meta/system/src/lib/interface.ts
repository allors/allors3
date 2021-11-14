import { Composite } from './composite';

export interface Interface extends Composite {
  readonly kind: 'Interface';
  _: unknown;
  subtypes: Set<Composite>;
}
