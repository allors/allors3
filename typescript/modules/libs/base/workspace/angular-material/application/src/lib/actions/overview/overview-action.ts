import { Subject } from 'rxjs';
import {
  Action,
  ActionTarget,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';

function objectTypeName(target: ActionTarget) {
  return Array.isArray(target)
    ? target.length > 0 && target[0].strategy.cls.singularName
    : target.strategy.cls.singularName;
}

export class OverviewAction implements Action {
  name = 'overview';

  constructor(private navigation: NavigationService) {}

  result = new Subject<boolean>();

  displayName = (target: ActionTarget) => 'Overview';

  description = (target: ActionTarget) =>
    `Go to ${objectTypeName(target)} overview`;

  disabled = (target: ActionTarget) => {
    if (Array.isArray(target)) {
      if (target.length > 0) {
        for (const item of target) {
          if (this.navigation.hasOverview(item)) {
            return false;
          }
        }
      }
    } else if (target) {
      return !this.navigation.hasOverview(target);
    }

    return true;
  };

  execute = (target: ActionTarget) => {
    if (Array.isArray(target)) {
      if (target.length > 0) {
        this.navigation.overview(target[0]);
      }
    } else {
      this.navigation.overview(target);
    }

    this.result.next(true);
  };
}
