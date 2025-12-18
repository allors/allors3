import { tags } from '@allors/default/workspace/meta';
import {
  AllorsMaterialDynamicCreateComponent as Create,
  AllorsMaterialDynamicEditComponent as Edit,
} from '@allors/base/workspace/angular-material/application';

export const dialogs = {
  create: {
    [tags.WorkTask]: Create,
  },
  edit: {},
};

export const components: any[] = [];
