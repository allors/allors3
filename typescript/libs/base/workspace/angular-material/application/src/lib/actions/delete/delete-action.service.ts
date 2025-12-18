import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  AllorsDialogService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  Action,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { DeleteAction } from './delete-action';

@Injectable({
  providedIn: 'root',
})
export class DeleteActionService {
  constructor(
    private workspaceService: WorkspaceService,
    private refreshService: RefreshService,
    private dialogService: AllorsDialogService,
    private errorService: ErrorService,
    private snackBar: MatSnackBar
  ) {}

  delete(): Action {
    return new DeleteAction(
      this.refreshService,
      this.dialogService,
      this.errorService,
      this.snackBar,
      this.workspaceService.contextBuilder()
    );
  }
}
