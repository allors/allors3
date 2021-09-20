import { FilterDefinition, IAngularMetaService } from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';
import { Sorter } from '@allors/workspace/angular/base';

export function configure(m: M, angularMeta: IAngularMetaService) {
  const angularPerson = angularMeta.for(m.Person);
  angularPerson.list = '/contacts/people';
  angularPerson.overview = '/contacts/person/:id';
  angularPerson.filterDefinition = new FilterDefinition({
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
  });
  angularPerson.sorter = new Sorter({
    firstName: m.Person.FirstName,
    lastName: m.Person.LastName,
    email: m.Person.UserEmail,
  });

  const angularOrganisation = angularMeta.for(m.Organisation);
  angularOrganisation.list = '/contacts/organisations';
  angularOrganisation.overview = '/contacts/organisation/:id';
  angularOrganisation.filterDefinition = new FilterDefinition({
    kind: 'And',
    operands: [
      {
        kind: 'Like',
        roleType: m.Organisation.Name,
        parameter: 'name',
      },
    ],
  });
  angularOrganisation.sorter = new Sorter({ name: m.Organisation.Name });
}
