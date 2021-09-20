import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RefreshService } from '../../../../services/refresh/refresh.service';

import { SaveService } from '../../save/save.service';
import { Action } from '../../../../components/actions/Action';

import { DeleteAction } from './DeleteAction';
import { AllorsMaterialDialogService } from '../../dialog/dialog.service';
import { SessionService } from '@allors/workspace/angular/core';

@Injectable({
  providedIn: 'root',
})
export class DeleteService {
  constructor(private refreshService: RefreshService, private dialogService: AllorsMaterialDialogService, private saveService: SaveService, private snackBar: MatSnackBar) {}

  delete(allors: SessionService): Action {
    return new DeleteAction(this.refreshService, this.dialogService, this.saveService, this.snackBar, allors);
  }
}
