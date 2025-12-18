import { Subject } from 'rxjs';
import {
  Action,
  ActionTarget,
  EditDialogService,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { OverviewAction } from '../overview/overview-action';
import { EditAction } from '../edit/edit-action';
import { OverviewActionService } from '../overview/overview-action.service';

function objectTypeName(target: ActionTarget) {
  return Array.isArray(target)
    ? target.length > 0 && target[0].strategy.cls.singularName
    : target.strategy.cls.singularName;
}

export class ViewAction implements Action {
  name = 'view';

  constructor(
    private overviewAction: OverviewAction,
    private editAction: EditAction
  ) {}

  result = new Subject<boolean>();

  displayName = (target: ActionTarget) => 'View';

  description = (target: ActionTarget) => `Go to ${objectTypeName(target)}`;

  disabled = (target: ActionTarget) =>
    this.editAction.disabled(target) && this.overviewAction.disabled(target);

  execute = (target: ActionTarget) => {
    if (!this.editAction.disabled(target)) {
      return this.editAction.execute(target);
    }

    return this.overviewAction.execute(target);
  };
}
