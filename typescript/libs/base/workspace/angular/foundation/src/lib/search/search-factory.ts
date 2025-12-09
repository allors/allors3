import { Observable, EMPTY } from 'rxjs';
import { map } from 'rxjs/operators';

import { RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  And,
  Or,
  TypeForParameter,
  IPullResult,
  Like,
  Pull,
} from '@allors/system/workspace/domain';
import { Context } from '@allors/base/workspace/angular/foundation';

import { SearchOptions } from './search-options';

const DEFAULT_TAKE = 100;

export class SearchFactory {
  constructor(private options: SearchOptions) {}

  public create(
    context: Context
  ): (
    search: string,
    parameters?: { [id: string]: TypeForParameter }
  ) => Observable<IObject[]> {
    return (
      search: string,
      parameters?: { [id: string]: TypeForParameter }
    ) => {
      if (search == null || !search.trim) {
        return EMPTY;
      }

      const terms: string[] = search.trim().split(' ');

      const and: And = { kind: 'And', operands: [] };

      if (this.options.post) {
        this.options.post(and);
      }

      if (this.options.predicates) {
        this.options.predicates.forEach((predicate) => {
          and.operands.push(predicate);
        });
      }

      terms.forEach((term: string) => {
        const or: Or = { kind: 'Or', operands: [] };
        and.operands.push(or);
        this.options.roleTypes.forEach((roleType: RoleType) => {
          const like: Like = {
            kind: 'Like',
            roleType,
            value: '%' + term + '%',
          };
          or.operands.push(like);
        });
      });

      const pulls: Pull[] = [
        {
          extent: {
            kind: 'Filter',
            objectType: this.options.objectType,
            predicate: and,
            sorting: this.options.roleTypes.map((roleType: RoleType) => {
              return {
                roleType,
              };
            }),
          },
          results: [
            {
              name: 'results',
              include: this.options.include,
              take: this.options.take ?? DEFAULT_TAKE,
            },
          ],
          arguments: parameters,
        },
      ];

      return context.pull(pulls).pipe(
        map((loaded: IPullResult) => {
          return loaded.collection<IObject>('results');
        })
      );
    };
  }
}
