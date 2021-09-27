import { Observable } from 'rxjs';
import { ActionTarget } from './action-target';
import { ActionResult } from './action-result';

export interface Action {
  name: string;
  displayName: (target: ActionTarget) => string;
  description: (target: ActionTarget) => string;
  disabled: (target: ActionTarget) => boolean;
  execute: (target: ActionTarget) => void;

  result: Observable<ActionResult>;
}
