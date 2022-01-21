import { DynamicEditComponent } from '@allors/base/workspace/angular-material/application';
import { tags } from '@allors/default/workspace/meta';

import { CountryEditComponent } from './domain/country/edit/country-edit.component';
import { EmploymentEditComponent } from './domain/employment/edit/employment-edit.component';
import { OrganisationCreateComponent } from './domain/organisation/create/organisation-create.component';
import { PersonCreateComponent } from './domain/person/create/person-create.component';

export const dialogs = {
  create: {
    [tags.Country]: DynamicEditComponent,
    [tags.Employment]: EmploymentEditComponent,
    [tags.Organisation]: OrganisationCreateComponent,
    [tags.Person]: PersonCreateComponent,
  },
  edit: {
    [tags.Country]: DynamicEditComponent,
    [tags.Employment]: EmploymentEditComponent,
  },
};

export const components: any[] = [
  CountryEditComponent,
  EmploymentEditComponent,
  OrganisationCreateComponent,
  PersonCreateComponent,
];
