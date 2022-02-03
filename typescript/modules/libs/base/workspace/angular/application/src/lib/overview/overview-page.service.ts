import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { Composite } from '@allors/system/workspace/meta';

export interface OverviewPageInfo {
  objectType: Composite;
  id: number;
}

@Injectable()
export class OverviewPageService {
  info$: Observable<OverviewPageInfo>;
}
