import { M } from '@allors/default/workspace/meta';
import {
  angularFilterDefinition,
  FilterDefinition,
} from '@allors/base/workspace/angular/foundation';
import {
  angularSorter,
  Sorter,
} from '@allors/base/workspace/angular-material/application';

export function configFilter(m: M) {
  angularFilterDefinition(
    m.Person,
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
  );

  angularSorter(
    m.Person,
    new Sorter({
      firstName: m.Person.FirstName,
      lastName: m.Person.LastName,
      email: m.Person.UserEmail,
    })
  );

  angularFilterDefinition(
    m.Organisation,
    new FilterDefinition({
      kind: 'And',
      operands: [
        {
          kind: 'Like',
          roleType: m.Organisation.Name,
          parameter: 'name',
        },
      ],
    })
  );
  angularSorter(m.Organisation, new Sorter({ name: m.Organisation.Name }));

  angularFilterDefinition(
    m.Country,
    new FilterDefinition({
      kind: 'And',
      operands: [
        {
          kind: 'Like',
          roleType: m.Country.IsoCode,
          parameter: 'name',
        },
        {
          kind: 'Like',
          roleType: m.Country.Name,
          parameter: 'name',
        },
      ],
    })
  );
  angularSorter(
    m.Country,
    new Sorter({ isoCode: m.Country.IsoCode, name: m.Country.Name })
  );
}
