import { Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Copyable } from '@allors/default/workspace/domain';
import {
  AllorsDialogService,
  Context,
} from '@allors/base/workspace/angular/foundation';
import {
  Action,
  ActionTarget,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';

export class CopyAction implements Action {
  name = 'copy';

  constructor(
    refreshService: RefreshService,
    dialogService: AllorsDialogService,
    errorService: ErrorService,
    snackBar: MatSnackBar,
    context: Context
  ) {
    this.execute = (target: ActionTarget) => {
      const copyables = Array.isArray(target)
        ? (target as Copyable[])
        : [target as Copyable];
      const methods = copyables
        .filter((v) => v.canExecuteCopy)
        .map((v) => v.Copy);

      if (methods.length > 0) {
        dialogService
          .confirm(
            methods.length === 1
              ? {
                  message: `Are you sure you want to copy this ${methods[0].object.strategy.cls.singularName}?`,
                }
              : { message: `Are you sure you want to copy these objects?` }
          )
          .subscribe((confirm: boolean) => {
            if (confirm) {
              context.invoke(methods).subscribe(() => {
                snackBar.open('Successfully copied.', 'close', {
                  duration: 5000,
                });
                refreshService.refresh();
                this.result.next(true);
              }, errorService.errorHandler);
            }
          });
      }
    };
  }

  result = new Subject<boolean>();

  execute: (target: ActionTarget) => void;

  displayName = () => 'Copy';
  description = () => 'Copy';
  disabled = (target: ActionTarget) => {
    if (Array.isArray(target)) {
      const anyDisabled = (target as Copyable[]).filter(
        (v) => !v.canExecuteCopy
      );
      return target.length > 0 ? anyDisabled.length > 0 : true;
    } else {
      return !(target as Copyable).canExecuteCopy;
    }
  };
}
