import { Composite } from '@allors/system/workspace/meta';
import { OnCreate } from '@allors/base/workspace/angular/application';

export interface CreateDialogData {
  readonly kind: 'CreateDialogData';
  objectType: Composite;
  onCreate?: OnCreate;
}
