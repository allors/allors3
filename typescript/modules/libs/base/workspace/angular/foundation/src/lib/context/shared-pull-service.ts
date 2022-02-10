import { ScopedPullHandler } from '@allors/system/workspace/domain';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SharedPullService {
  handlers: Set<ScopedPullHandler>;

  constructor() {
    this.handlers = new Set();
  }

  register(pull: ScopedPullHandler) {
    this.handlers.add(pull);
  }

  unregister(pull: ScopedPullHandler) {
    this.handlers.delete(pull);
  }
}
