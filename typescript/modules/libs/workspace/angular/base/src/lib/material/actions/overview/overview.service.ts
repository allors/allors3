import { Injectable } from '@angular/core';
import { NavigationService } from '../../../navigation/navigation.service';
import { Action } from '../../../actions/action';
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
