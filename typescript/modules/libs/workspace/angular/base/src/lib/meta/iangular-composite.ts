import { Filter } from '../components/filter/filter';
import { FilterDefinition } from '../components/filter/filter-definition';
import { Sorter } from '../material/sorting/sorter';

export interface IAngularComposite {
  readonly kind: 'AngularComposite';
  displayName?: string;
  icon?: string;
  list?: string;
  overview?: string;
  filterDefinition?: FilterDefinition;
  filter?: Filter;
  sorter?: Sorter;
}
