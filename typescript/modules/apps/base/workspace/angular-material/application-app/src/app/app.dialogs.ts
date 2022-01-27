import {
  DynamicCreateComponent,
  DynamicEditComponent,
} from '@allors/base/workspace/angular-material/application';
import { tags } from '@allors/default/workspace/meta';

import { EmploymentEditComponent } from './domain/employment/form/employment-form.component';
import { OrganisationCreateComponent } from './domain/organisation/create/organisation-create.component';
import { PersonCreateComponent } from './domain/person/create/person-create.component';

export const dialogs = {
  create: {
    [tags.Country]: DynamicCreateComponent,
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
  EmploymentEditComponent,
  OrganisationCreateComponent,
  PersonCreateComponent,
];
