import { Injectable } from '@angular/core';
import { ViewAction } from './view-action';
import { OverviewActionService } from '../overview/overview-action.service';
import { EditActionService } from '../edit/edit-action.service';

@Injectable({
  providedIn: 'root',
})
export class ViewActionService {
  constructor(
    private overviewActionService: OverviewActionService,
    private editActionService: EditActionService
  ) {}

  view(): ViewAction {
    return new ViewAction(
      this.overviewActionService.overview(),
      this.editActionService.edit()
    );
  }
}
