import { Injectable } from '@angular/core';
import {
  Action,
  NavigationService,
} from '@allors/base/workspace/angular/application';
import { OverviewAction } from './overview-action';

@Injectable({
  providedIn: 'root',
})
export class OverviewService {
  constructor(private navigationService: NavigationService) {}

  overview(): Action {
    return new OverviewAction(this.navigationService);
  }
}
