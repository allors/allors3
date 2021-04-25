import { Composite } from './Composite';

export interface Interface extends Composite {
  subtypes: Readonly<Set<Composite>>;
}
