import { Subject } from 'rxjs';
import { RoleType } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import {
  Action,
  ActionTarget,
  EditRequest,
  EditService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';

export class EditRoleAction implements Action {
  name = 'edit';
  result = new Subject<boolean>();
  displayName = () => 'Edit';
  description = () => 'Edit';

  constructor(
    private editService: EditService,
    private refreshService: RefreshService,
    private roleType?: RoleType
  ) {}

  resolve(target: ActionTarget): IObject {
    let editObject = target as IObject;

    if (this.roleType) {
      editObject = editObject.strategy.getCompositeRole(this.roleType);
    }

    return editObject;
  }

  disabled(target: ActionTarget) {
    const editObject = this.resolve(target);
    return !this.editService.canEdit(editObject);
  }

  execute(target: ActionTarget) {
    const resolved = this.resolve(target);

    const request: EditRequest = {
      kind: 'EditRequest',
      objectId: resolved.id,
      objectType: resolved.strategy.cls,
    };

    this.editService.edit(request).subscribe(() => {
      this.refreshService.refresh();
      this.result.next(true);
    });
  }
}
