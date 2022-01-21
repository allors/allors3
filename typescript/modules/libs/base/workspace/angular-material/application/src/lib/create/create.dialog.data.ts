import { Composite } from '@allors/system/workspace/meta';
import { ISession } from '@allors/system/workspace/domain';
import { OnCreate } from '@allors/workspace/angular/base';

export interface CreateDialogData {
  readonly kind: 'CreateDialogData';
  session: ISession;
  objectType: Composite;
  onCreate?: OnCreate;
}
