import { Sort as MaterialSort } from '@angular/material/sort';
import {
  Sort as AllorsSort,
  SortDirection,
} from '@allors/system/workspace/domain';
import { RoleType } from '@allors/system/workspace/meta';

export class Sorter {
  private config: { [index: string]: RoleType | RoleType[] };

  constructor(config: { [index: string]: RoleType | RoleType[] }) {
    this.config = config;
  }

  create(sort: MaterialSort): AllorsSort[] {
    if (sort) {
      const sortDirection =
        sort.direction === 'desc'
          ? SortDirection.Descending
          : SortDirection.Ascending;
      const roleTypeOrRoleTypes = this.config[sort.active];

      if (roleTypeOrRoleTypes instanceof Array) {
        return (roleTypeOrRoleTypes as RoleType[]).map((v) => {
          return {
            roleType: v as RoleType,
            sortDirection,
          };
        });
      } else {
        return [
          {
            roleType: roleTypeOrRoleTypes as RoleType,
            sortDirection,
          },
        ];
      }
    }

    return null;
  }
}
