import { M } from '@allors/workspace/meta/default';
import { Composite } from '@allors/system/workspace/meta';
import {
  angularFilterDefinition,
  angularForms,
  angularList,
  angularMenu,
  angularOverview,
  FilterDefinition,
} from '@allors/workspace/angular/base';
import { angularSorter, Sorter } from '@allors/workspace/angular-material/base';
import { CountryFormComponent } from './domain/country/forms/country-form.component';

function nav(composite: Composite, list: string, overview?: string) {
  angularList(composite, list);
  angularOverview(composite, overview);
}

function forms(composite: Composite, both: unknown);
function forms(composite: Composite, create: unknown, edit: unknown);
function forms(composite: Composite, bothOrCreate: unknown, edit?: unknown) {
  if (edit == null) {
    angularForms(composite, { create: bothOrCreate, edit: bothOrCreate });
  } else {
    angularForms(composite, { create: bothOrCreate, edit: edit });
  }
}

export function configure(m: M) {
  // Menu
  angularMenu(m, [
    { title: 'Home', icon: 'home', link: '/' },
    {
      title: 'Contacts',
      icon: 'business',
      children: [
        { objectType: m.Person },
        { objectType: m.Organisation },
        { objectType: m.Country },
      ],
    },
    { title: 'Fields', icon: 'build', link: '/fields' },
  ]);

  // Navigation
  nav(m.Person, '/contacts/people', '/contacts/person/:id');
  nav(m.Organisation, '/contacts/organisations', '/contacts/organisation/:id');
  nav(m.Country, '/contacts/countries');

  // Forms
  forms(m.Country, CountryFormComponent);

  // Filter & Sort
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
