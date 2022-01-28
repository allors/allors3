import {
  AllorsMaterialDynamicCreateComponent as Create,
  AllorsMaterialDynamicEditComponent as Edit,
} from '@allors/base/workspace/angular-material/application';
import { tags } from '@allors/default/workspace/meta';

import { EmploymentFormComponent } from './domain/employment/form/employment-form.component';
import { OrganisationCreateComponent } from './domain/organisation/create/organisation-create.component';
import { PersonCreateComponent } from './domain/person/create/person-create.component';

export const dialogs = {
  create: {
    [tags.Country]: Create,
    [tags.Employment]: Create,
    [tags.Organisation]: OrganisationCreateComponent,
    [tags.Person]: PersonCreateComponent,
  },
  edit: {
    [tags.Country]: Edit,
    [tags.Employment]: Edit,
  },
};

export const components: any[] = [
  EmploymentFormComponent,
  OrganisationCreateComponent,
  PersonCreateComponent,
];
