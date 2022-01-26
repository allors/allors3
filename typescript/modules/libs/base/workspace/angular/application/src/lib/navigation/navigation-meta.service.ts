import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { IObject } from '@allors/system/workspace/domain';
import { Composite } from '@allors/system/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

import { angularPageList } from '../meta/angular-page-list';
import { angularPageEdit } from '../meta/angular-page-edit';

import { NavigationService } from './navigation.service';

@Injectable({
  providedIn: 'root',
})
export class NavigationMetaService extends NavigationService {
  constructor(
    private router: Router,
    private workspaceService: WorkspaceService
  ) {
    super();
  }

  hasList(obj: IObject): boolean {
    const list = obj ? angularPageList(obj.strategy.cls) : null;
    return list != null;
  }

  list(objectType: Composite) {
    const url = angularPageList(objectType);
    if (url != null) {
      this.router.navigate([url]);
    }
  }

  hasOverview(obj: IObject): boolean {
    const overview = obj ? angularPageEdit(obj.strategy.cls) : null;
    return overview != null;
  }

  overview(object: IObject) {
    const overview = object ? angularPageEdit(object.strategy.cls) : null;
    const url = overview?.replace(`:id`, object.strategy.id.toString());
    if (url != null) {
      this.router.navigate([url]);
    }
  }
}
