import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Context } from '@allors/base/workspace/angular/foundation';
import { MethodType } from '@allors/system/workspace/meta';
import {
  RefreshService,
  SaveService,
} from '@allors/base/workspace/angular/foundation';
import { Action } from '@allors/base/workspace/angular/application';
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
