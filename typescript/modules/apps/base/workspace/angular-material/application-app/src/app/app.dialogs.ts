import {
  AllorsMaterialDynamicCreateComponent as Create,
  AllorsMaterialDynamicEditComponent as Edit,
} from '@allors/base/workspace/angular-material/application';
import { tags } from '@allors/default/workspace/meta';

import { PersonCreateComponent } from './domain/person/create/person-create.component';

export const dialogs = {
  create: {
    [tags.Country]: Create,
    [tags.Employment]: Create,
    [tags.Organisation]: Create,
    [tags.Person]: PersonCreateComponent,
  },
  edit: {
    [tags.Country]: Edit,
    [tags.Employment]: Edit,
  },
};

export const components: any[] = [PersonCreateComponent];
