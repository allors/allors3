import {
  Filter,
  FilterDefinition,
  FilterService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M, tags } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppFilterService implements FilterService {
  m: M;

  countryFilter: Filter;
  organisationFilter: Filter;
  personFilter: Filter;

  constructor(workspaceService: WorkspaceService) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  filter(composite: Composite): Filter {
    const { m } = this;

    switch (composite.tag) {
      case tags.Country:
        return (this.countryFilter ??= new Filter(
          new FilterDefinition({
            kind: 'And',
            operands: [
              {
                kind: 'Like',
                roleType: m.Country.Name,
                parameter: 'name',
              },
            ],
          })
        ));

      case tags.Organisation:
        return (this.organisationFilter ??= new Filter(
          new FilterDefinition({
            kind: 'And',
            operands: [
              {
                kind: 'Like',
                roleType: m.Organisation.Name,
                parameter: 'name',
              },
              {
                kind: 'Like',
                roleType: m.Country.Name,
                parameter: 'country',
              },
            ],
          })
        ));

      case tags.Person:
        return (this.personFilter ??= new Filter(
          new FilterDefinition({
            kind: 'And',
            operands: [
              {
                kind: 'Like',
                roleType: m.Person.FirstName,
                parameter: 'firstName',
              },
              {
                kind: 'Like',
                roleType: m.Person.LastName,
                parameter: 'lastName',
              },
              {
                kind: 'Like',
                roleType: m.Person.UserEmail,
                parameter: 'email',
              },
            ],
          })
        ));
    }

    return null;
  }
}
