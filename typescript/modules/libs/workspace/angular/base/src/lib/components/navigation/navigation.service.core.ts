import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { IObject } from '@allors/workspace/domain/system';
import { Composite } from '@allors/workspace/meta/system';
import { WorkspaceService } from '@allors/workspace/angular/core';

import { NavigationService } from '../../services/navigation/navigation.service';

@Injectable({
  providedIn: 'root',
})
export class NavigationServiceCore extends NavigationService {
  constructor(private router: Router, private workspaceService: WorkspaceService) {
    super();
  }

  hasList(obj: IObject): boolean {
    const list = obj?.strategy.cls._.list;
    return list != null;
  }

  list(objectType: Composite) {
    const url = objectType._.list;
    if (url != null) {
      this.router.navigate([url]);
    }
  }

  hasOverview(obj: IObject): boolean {
    const overview = obj?.strategy.cls._.overview;
    return overview != null;
  }

  overview(obj: IObject) {
    const overview = obj?.strategy.cls._.overview;
    const url = overview?.replace(`:id`, obj.strategy.id.toString());
    if (url != null) {
      this.router.navigate([url]);
    }
  }
}
