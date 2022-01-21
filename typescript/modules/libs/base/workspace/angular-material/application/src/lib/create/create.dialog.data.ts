import { Composite } from '@allors/system/workspace/meta';
import { ISession } from '@allors/system/workspace/domain';
import { OnCreate } from '@allors/base/workspace/angular/foundation';

export interface CreateDialogData {
  readonly kind: 'CreateDialogData';
  session: ISession;
  objectType: Composite;
  onCreate?: OnCreate;
}
