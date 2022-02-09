import { Injectable } from '@angular/core';

import { RoleType } from '@allors/system/workspace/meta';
import { Action } from '@allors/base/workspace/angular/application';

import { PrintAction } from './print-action';

export class PrintConfig {
  url: string;
}

@Injectable()
export class PrintService {
  constructor(public config: PrintConfig) {}

  print(roleType?: RoleType): Action {
    return new PrintAction(this.config, roleType);
  }
}
