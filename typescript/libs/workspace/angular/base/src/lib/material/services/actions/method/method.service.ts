import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { MethodType } from '@allors/workspace/meta/system';

import { RefreshService } from '../../../../services/refresh/refresh.service';
import { SaveService } from '../../save/save.service';
import { Action } from '../../../../components/actions/Action';
import { Context } from '../../../../services/framework/Context';

import { MethodAction } from './MethodAction';
import { MethodConfig } from './MethodConfig';

@Injectable({
  providedIn: 'root',
})
export class MethodService {
  constructor(private refreshService: RefreshService, private saveService: SaveService, private snackBar: MatSnackBar) {}

  create(context: Context, methodType: MethodType, config?: MethodConfig): Action {
    return new MethodAction(this.refreshService, this.snackBar, context, this.saveService, methodType, config);
  }
}
