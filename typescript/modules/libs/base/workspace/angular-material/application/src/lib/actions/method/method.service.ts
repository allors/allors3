import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Context } from '@allors/workspace/angular/core';
import { MethodType } from '@allors/system/workspace/meta';
import {
  Action,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';
import { MethodAction } from './method-action';
import { MethodConfig } from './method-config';

@Injectable({
  providedIn: 'root',
})
export class MethodService {
  constructor(
    private refreshService: RefreshService,
    private saveService: SaveService,
    private snackBar: MatSnackBar
  ) {}

  create(
    context: Context,
    methodType: MethodType,
    config?: MethodConfig
  ): Action {
    return new MethodAction(
      this.refreshService,
      this.snackBar,
      context,
      this.saveService,
      methodType,
      config
    );
  }
}
