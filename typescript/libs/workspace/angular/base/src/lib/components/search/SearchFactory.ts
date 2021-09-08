import { Observable, EMPTY } from 'rxjs';
import { map } from 'rxjs/operators';

import { RoleType } from '@allors/workspace/meta/system';
import { IObject, And, Like, Or, Pull, Sort, ISession, TypeForParameter } from '@allors/workspace/domain/system';

import { SearchOptions } from './SearchOptions';
import { SessionService } from '@allors/workspace/angular/core';

export class SearchFactory {
  constructor(private options: SearchOptions) {}

  public create(sessionOrSessionService: ISession | SessionService): (search: string, parameters?: { [id: string]: TypeForParameter }) => Observable<IObject[]> {
    return (search: string, parameters?: { [id: string]: TypeForParameter }) => {
      if (search === undefined || search === null || !search.trim) {
        return EMPTY;
      }

      const terms: string[] = search.trim().split(' ');

      const and: Partial<And> = {};

      if (this.options.post) {
        this.options.post(and);
      }

      if (this.options.predicates) {
        this.options.predicates.forEach((predicate) => {
          and.operands.push(predicate);
        });
      }

      terms.forEach((term: string) => {
        const or: Or = new Or();
        and.operands.push(or);
        this.options.roleTypes.forEach((roleType: RoleType) => {
          or.operands.push(new Like({ roleType, value: '%' + term + '%' }));
        });
      });

      const pulls = [
        new Pull(this.options.objectType, {
          name: 'results',
          predicate: and,
          sort: this.options.roleTypes.map((roleType: RoleType) => new Sort({ roleType })),
          parameters,
          include: this.options.include,
        }),
      ];

      const context = contextOrService instanceof Context ? contextOrService : contextOrService.context;

      return context.load(new PullRequest({ pulls })).pipe(
        map((loaded: IPullResult) => {
          return loaded.collections.results;
        })
      );
    };
  }
}
