import { Subject } from 'rxjs';
import { RoleType } from '@allors/system/workspace/meta';
import { IObject } from '@allors/workspace/domain/system';
import {
  Action,
  ActionTarget,
  EditService,
  RefreshService,
} from '@allors/workspace/angular/base';

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

  resolve(target: ActionTarget) {
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
    const editObject = this.resolve(target);
    this.editService.edit(editObject).subscribe(() => {
      this.refreshService.refresh();
      this.result.next(true);
    });
  }
}
