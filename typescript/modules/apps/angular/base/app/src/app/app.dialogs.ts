import { tags } from '@allors/workspace/meta/default';

import { CountryEditComponent } from './objects/country/edit/country-edit.component';
import { EmploymentEditComponent } from './objects/employment/edit/employment-edit.component';
import { OrganisationCreateComponent } from './objects/organisation/create/organisation-create.component';
import { PersonCreateComponent } from './objects/person/create/person-create.component';

export const dialogs = {
  create: {
    [tags.Country]: CountryEditComponent,
    [tags.Employment]: EmploymentEditComponent,
    [tags.Organisation]: OrganisationCreateComponent,
    [tags.Person]: PersonCreateComponent,
  },
  edit: {
    [tags.Country]: CountryEditComponent,
    [tags.Employment]: EmploymentEditComponent,
  },
};

export const components: any[] = [CountryEditComponent, EmploymentEditComponent, OrganisationCreateComponent, PersonCreateComponent];
