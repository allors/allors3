import { Subject } from 'rxjs';

import { RoleType } from '@allors/system/workspace/meta';
import { Printable } from '@allors/default/workspace/domain';
import {
  Action,
  ActionTarget,
} from '@allors/base/workspace/angular/foundation';

import { PrintConfig } from './print.service';

export class PrintAction implements Action {
  name = 'print';

  constructor(config: PrintConfig, roleType?: RoleType) {
    this.execute = (target: ActionTarget) => {
      let printable = target as Printable;

      if (roleType) {
        printable = printable.strategy.getRole(roleType) as Printable;
      }

      const url = `${config.url}print/${printable.id}`;
      window.open(url, '_blank', 'noopener');
    };
  }

  result = new Subject<boolean>();

  execute: (target: ActionTarget) => void;

  displayName = () => 'Print';
  description = () => 'Print';
  disabled = (target: ActionTarget) => {
    if (Array.isArray(target)) {
      return true;
    } else {
      return (
        !(target as Printable).canReadPrintDocument ||
        !(target as Printable).PrintDocument
      );
    }
  };
}
