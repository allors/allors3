import { SharedPullHandler } from '@allors/system/workspace/domain';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SharedPullService {
  handlers: Set<SharedPullHandler>;

  constructor() {
    this.handlers = new Set();
  }

  register(pull: SharedPullHandler) {
    this.handlers.add(pull);
  }

  unregister(pull: SharedPullHandler) {
    this.handlers.delete(pull);
  }
}
