import { Injectable } from '@angular/core';

import { Action } from '../../../../components/actions/Action';
import { NavigationService } from '../../../../services/navigation/navigation.service';

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
