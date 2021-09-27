import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { MethodType } from '@allors/workspace/meta/system';
import { SessionService } from '@allors/workspace/angular/core';

import { RefreshService } from '../../../../services/refresh/refresh.service';
import { SaveService } from '../../save/save.service';
import { Action } from '../../../../components/actions/action';

import { MethodAction } from './method-action';
import { MethodConfig } from './method-config';

@Injectable({
  providedIn: 'root',
})
export class MethodService {
  constructor(private refreshService: RefreshService, private saveService: SaveService, private snackBar: MatSnackBar) {}

  create(allors: SessionService, methodType: MethodType, config?: MethodConfig): Action {
    return new MethodAction(this.refreshService, this.snackBar, allors, this.saveService, methodType, config);
  }
}
