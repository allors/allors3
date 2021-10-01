import { FilterDefinition } from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';
import { Sorter } from '@allors/workspace/angular/base';
import { Composite } from '@allors/workspace/meta/system';

function nav(composite: Composite, list: string, overview?: string) {
  composite._.list = list;
  composite._.overview = overview;
}

export function configure(m: M) {
  // Menu
  m._.menu = [
    { title: 'Home', icon: 'home', link: '/' },
    {
      title: 'Contacts',
      icon: 'business',
      children: [{ objectType: m.Person }, { objectType: m.Organisation }],
    },
    {
      title: 'Tests',
      icon: 'build',
      children: [{ title: 'Form', icon: 'share', link: '/tests/form' }],
    },
  ];

  // Navigation
  nav(m.Person, '/contacts/people', '/contacts/person/:id');
  nav(m.Organisation, '/contacts/organisations', '/contacts/organisation/:id');

  // Filter & Sort
  m.Person._.filterDefinition = new FilterDefinition({
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
  m.Person._.sorter = new Sorter({
    firstName: m.Person.FirstName,
    lastName: m.Person.LastName,
    email: m.Person.UserEmail,
  });

  m.Organisation._.filterDefinition = new FilterDefinition({
    kind: 'And',
    operands: [
      {
        kind: 'Like',
        roleType: m.Organisation.Name,
        parameter: 'name',
      },
    ],
  });
  m.Organisation._.sorter = new Sorter({ name: m.Organisation.Name });
}
