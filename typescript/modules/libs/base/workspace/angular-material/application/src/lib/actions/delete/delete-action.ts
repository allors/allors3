import { Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Deletable } from '@allors/default/workspace/domain';
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

export class DeleteAction implements Action {
  name = 'delete';

  constructor(
    refreshService: RefreshService,
    dialogService: AllorsDialogService,
    errorService: ErrorService,
    snackBar: MatSnackBar,
    context: Context
  ) {
    this.execute = (target: ActionTarget) => {
      const deletables = Array.isArray(target)
        ? (target as Deletable[])
        : [target as Deletable];
      const methods = deletables
        .filter((v) => v.canExecuteDelete)
        .map((v) => v.Delete);

      if (methods.length > 0) {
        dialogService
          .confirm(
            methods.length === 1
              ? {
                  message: `Are you sure you want to delete this ${methods[0].object.strategy.cls.singularName}?`,
                }
              : { message: `Are you sure you want to delete these objects?` }
          )
          .subscribe((confirm: boolean) => {
            if (confirm) {
              context.invoke(methods).subscribe(() => {
                snackBar.open('Successfully deleted.', 'close', {
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

  displayName = () => 'Delete';
  description = () => 'Delete';
  disabled = (target: ActionTarget) => {
    if (Array.isArray(target)) {
      const anyDisabled = (target as Deletable[]).filter(
        (v) => !v.canExecuteDelete
      );
      return target.length > 0 ? anyDisabled.length > 0 : true;
    } else {
      return !(target as Deletable).canExecuteDelete;
    }
  };
}
