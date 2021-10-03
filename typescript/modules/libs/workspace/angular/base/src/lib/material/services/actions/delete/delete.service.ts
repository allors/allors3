import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { Context } from '@allors/workspace/angular/core';

import { RefreshService } from '../../../../services/refresh/refresh.service';
import { SaveService } from '../../save/save.service';
import { AllorsMaterialDialogService } from '../../dialog/dialog.service';
import { Action } from '../../../../components/actions/action';

import { DeleteAction } from './delete-action';

@Injectable({
  providedIn: 'root',
})
export class DeleteService {
  constructor(private refreshService: RefreshService, private dialogService: AllorsMaterialDialogService, private saveService: SaveService, private snackBar: MatSnackBar) {}

  delete(context: Context): Action {
    return new DeleteAction(this.refreshService, this.dialogService, this.saveService, this.snackBar, context);
  }
}
