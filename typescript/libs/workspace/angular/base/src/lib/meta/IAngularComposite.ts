import { Filter } from '../components/filter/Filter';
import { FilterDefinition } from '../components/filter/FilterDefinition';
import { Sorter } from '../material/sorting/Sorter';

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
