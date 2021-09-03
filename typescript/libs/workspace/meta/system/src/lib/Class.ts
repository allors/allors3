import { Composite } from './Composite';

export interface Class extends Composite {
  readonly kind: 'Class';
}
