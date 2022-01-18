import { Composite } from '@allors/workspace/meta/system';
import { ISession } from '@allors/workspace/domain/system';
import { OnCreate } from '@allors/workspace/angular/base';

export interface CreateDialogData {
  readonly kind: 'CreateDialogData';
  session: ISession;
  objectType: Composite;
  onCreate?: OnCreate;
}
