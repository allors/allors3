import { PropertyType } from '@allors/system/workspace/meta';

import { SearchFactory } from '../search/search-factory';

export class FilterOptions {
  search: () => SearchFactory;
  display: (v: any) => string;
  initialValue: any | ((x: any) => any);

  exist: PropertyType;

  constructor(fields: Partial<FilterOptions>) {
    Object.assign(this, fields);

    if (!this.display) {
      this.display = (v: any) => {
        if (v.display) {
          return v.display;
        }

        return v.toString();
      };
    }
  }
}
