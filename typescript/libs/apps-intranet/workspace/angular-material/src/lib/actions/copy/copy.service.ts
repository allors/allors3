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
import { CopyAction } from './copy-action';

@Injectable({
  providedIn: 'root',
})
export class CopyService {
  constructor(
    private workspaceService: WorkspaceService,
    private refreshService: RefreshService,
    private dialogService: AllorsDialogService,
    private errorService: ErrorService,
    private snackBar: MatSnackBar
  ) {}

  copy(): Action {
    return new CopyAction(
      this.refreshService,
      this.dialogService,
      this.errorService,
      this.snackBar,
      this.workspaceService.contextBuilder()
    );
  }
}
