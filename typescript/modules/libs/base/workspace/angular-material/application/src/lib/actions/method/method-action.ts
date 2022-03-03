import { Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MethodType } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import {
  Context,
  ContextService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  Action,
  ActionTarget,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { MethodConfig } from './method-config';

export class MethodAction implements Action {
  name = 'method';

  constructor(
    workspaceService: WorkspaceService,
    refreshService: RefreshService,
    snackBar: MatSnackBar,
    errorService: ErrorService,
    public methodType: MethodType,
    public config?: MethodConfig
  ) {
    this.execute = (target: ActionTarget) => {
      const objects = this.resolve(target);
      const methods = objects
        .filter((v) => v.strategy.canExecute(methodType))
        .map((v) => (v as any)[methodType.name]);

      const context = workspaceService.contextBuilder();

      if (methods.length > 0) {
        context.invoke(methods).subscribe(() => {
          snackBar.open(
            'Successfully executed ' + methodType.name + '.',
            'close',
            { duration: 5000 }
          );
          refreshService.refresh();
          this.result.next(true);
        }, errorService.errorHandler);
      }
    };
  }

  result = new Subject<boolean>();

  execute: (target: ActionTarget) => void;

  displayName = () => (this.config && this.config.name) || this.methodType.name;
  description = () =>
    (this.config && this.config.description) || this.methodType.name;
  disabled = (target: ActionTarget) => {
    const objects = this.resolve(target);
    return objects?.find((v) => v.strategy.canExecute(this.methodType)) == null;
  };

  private resolve(target: ActionTarget): IObject[] {
    let objects = Array.isArray(target)
      ? (target as IObject[])
      : [target as IObject];

    const path = this.config?.path;
    if (path) {
      objects = objects.map((v) => {
        for (const roleType of path) {
          v = v.strategy.getCompositeRole(roleType);
        }

        return v;
      });
    }

    return objects;
  }
}
