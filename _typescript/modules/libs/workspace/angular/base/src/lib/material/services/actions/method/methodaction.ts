import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';

import { MethodType } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';
import { SessionService } from '@allors/workspace/angular/core';

import { Action } from '../../../../components/actions/Action';
import { RefreshService } from '../../../../services/refresh/refresh.service';
import { SaveService } from '../../save/save.service';
import { ActionTarget } from '../../../../components/actions/ActionTarget';

import { MethodConfig } from './MethodConfig';

export class MethodAction implements Action {
  name = 'method';

  constructor(refreshService: RefreshService, snackBar: MatSnackBar, allors: SessionService, saveService: SaveService, public methodType: MethodType, public config?: MethodConfig) {
    this.execute = (target: ActionTarget) => {
      const objects = this.resolve(target);
      const methods = objects.filter((v) => v.strategy.canExecute(methodType)).map((v) => (v as any)[methodType.name]);

      if (methods.length > 0) {
        allors.client.invokeReactive(allors.session, methods).subscribe(() => {
          snackBar.open('Successfully executed ' + methodType.name + '.', 'close', { duration: 5000 });
          refreshService.refresh();
          this.result.next(true);
        }, saveService.errorHandler);
      }
    };
  }

  result = new Subject<boolean>();

  execute: (target: ActionTarget) => void;

  displayName = () => (this.config && this.config.name) || this.methodType.name;
  description = () => (this.config && this.config.description) || this.methodType.name;
  disabled = (target: ActionTarget) => {
    const objects = this.resolve(target);
    return objects?.find((v) => v.strategy.canExecute(this.methodType)) == null;
  };

  private resolve(target: ActionTarget): IObject[] {
    let objects = Array.isArray(target) ? (target as IObject[]) : [target as IObject];

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
