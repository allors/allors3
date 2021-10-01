import { Filter } from '../components/filter/filter';
import { FilterDefinition } from '../components/filter/filter-definition';
import { Sorter } from '../material/sorting/sorter';

declare module '@allors/workspace/meta/system' {
  interface RoleTypeExtension {
    displayName?: string;
  }

  interface CompositeExtension {
    displayName?: string;
    icon?: string;
    list?: string;
    overview?: string;
    filterDefinition?: FilterDefinition;
    filter?: Filter;
    sorter?: Sorter;
  }
}
