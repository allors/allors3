import { Injectable } from '@angular/core';
import { Action } from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { OverviewAction } from './overview-action';

@Injectable({
  providedIn: 'root',
})
export class OverviewService {
  constructor(private navigation: NavigationService) {}

  overview(): Action {
    return new OverviewAction(this.navigation);
  }
}
