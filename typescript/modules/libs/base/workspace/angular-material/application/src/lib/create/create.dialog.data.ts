import { Composite } from '@allors/system/workspace/meta';
import { OnCreate } from '@allors/base/workspace/angular/foundation';

export interface CreateDialogData {
  readonly kind: 'CreateDialogData';
  objectType: Composite;
  onCreate?: OnCreate;
}
