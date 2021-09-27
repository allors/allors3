import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { IObject } from '@allors/workspace/domain/system';
import { Composite } from '@allors/workspace/meta/system';
import { WorkspaceService } from '@allors/workspace/angular/core';

import { NavigationService } from '../../services/navigation/navigation.service';
import { IAngularMetaService } from '../../meta/iangular-meta-service';

@Injectable({
  providedIn: 'root',
})
export class NavigationServiceCore extends NavigationService {
  readonly angularMeta: IAngularMetaService;

  constructor(private router: Router, private workspaceService: WorkspaceService) {
    super();

    this.angularMeta = this.workspaceService.workspace.services.angularMetaService;
  }

  hasList(obj: IObject): boolean {
    const angularClass = this.angularMeta.for(obj?.strategy.cls);
    return angularClass?.list != null;
  }

  list(objectType: Composite) {
    const angularComposite = this.angularMeta.for(objectType);
    const url = angularComposite?.list;
    if (url != null) {
      this.router.navigate([url]);
    }
  }

  hasOverview(obj: IObject): boolean {
    const angularClass = this.angularMeta.for(obj?.strategy.cls);
    return angularClass?.overview != null;
  }

  overview(obj: IObject) {
    const angularComposite = this.angularMeta.for(obj.strategy.cls);
    const url = angularComposite?.overview.replace(`:id`, obj.strategy.id.toString());
    if (url != null) {
      this.router.navigate([url]);
    }
  }
}
