import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Context } from '@allors/base/workspace/angular/foundation';
import {
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { Action } from '@allors/base/workspace/angular/application';
import { AllorsMaterialDialogService } from '../../dialog/dialog.service';
import { DeleteAction } from './delete-action';

@Injectable({
  providedIn: 'root',
})
export class DeleteService {
  constructor(
    private refreshService: RefreshService,
    private dialogService: AllorsMaterialDialogService,
    private errorService: ErrorService,
    private snackBar: MatSnackBar
  ) {}

  delete(context: Context): Action {
    return new DeleteAction(
      this.refreshService,
      this.dialogService,
      this.errorService,
      this.snackBar,
      context
    );
  }
}
