import { Subject } from 'rxjs';

import { PrintConfig } from './print.service';
import { Action, ActionTarget } from '@allors/workspace/angular/base';
import { RoleType } from '@allors/workspace/meta/system';
import { Printable } from '@allors/workspace/domain/default';

export class PrintAction implements Action {
  name = 'print';

  constructor(config: PrintConfig, roleType?: RoleType) {
    this.execute = (target: ActionTarget) => {
      let printable = target as Printable;

      if (roleType) {
        printable = printable.strategy.getRole(roleType) as Printable;
      }

      const url = `${config.url}print/${printable.id}`;
      window.open(url);
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
      return !(target as Printable).canReadPrintDocument || !(target as Printable).PrintDocument;
    }
  };
}
