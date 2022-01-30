import { Injectable } from '@angular/core';
import { OnPull } from '@allors/system/workspace/domain';

@Injectable({
  providedIn: 'root',
})
export class OnPullService {
  onPulls: Set<OnPull>;

  constructor() {
    this.onPulls = new Set();
  }

  register(pull: OnPull) {
    this.onPulls.add(pull);
  }

  unregister(pull: OnPull) {
    this.onPulls.delete(pull);
  }
}
