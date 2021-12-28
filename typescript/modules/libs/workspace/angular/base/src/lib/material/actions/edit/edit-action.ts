import { Subject } from 'rxjs';

import { RoleType } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';

import { ObjectService } from '../../object/object.service';
import { Action } from '../../../actions/action';
import { RefreshService } from '../../../refresh/refresh.service';
import { ActionTarget } from '../../../actions/action-target';

export class EditAction implements Action {
  name = 'edit';
  result = new Subject<boolean>();
  displayName = () => 'Edit';
  description = () => 'Edit';

  constructor(private objectService: ObjectService, private refreshService: RefreshService, private roleType?: RoleType) {}

  resolve(target: ActionTarget) {
    let editObject = target as IObject;

    if (this.roleType) {
      editObject = editObject.strategy.getCompositeRole(this.roleType);
    }

    return editObject;
  }

  disabled(target: ActionTarget) {
    const editObject = this.resolve(target);
    return !this.objectService.hasEditControl(editObject);
  }

  execute(target: ActionTarget) {
    const editObject = this.resolve(target);
    this.objectService.edit(editObject).subscribe(() => {
      this.refreshService.refresh();
      this.result.next(true);
    });
  }
}
