import { NavigationService } from '@allors/base/workspace/angular/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AppNavigationService extends NavigationService {
  listByComposite: Map<Composite, string>;
  overviewByComposite: Map<Composite, any>;

  constructor(private router: Router, workspaceService: WorkspaceService) {
    super();

    this.listByComposite = new Map();
    this.overviewByComposite = new Map();

    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.listByComposite.set(m.Person, '/contacts/people');
    this.listByComposite.set(m.Organisation, '/contacts/organisations');
    this.listByComposite.set(m.Country, '/contacts/countries');

    this.overviewByComposite.set(m.Person, '/contacts/person/:id');
    this.overviewByComposite.set(m.Organisation, '/contacts/organisation/:id');
  }

  hasList(objectType: Composite): boolean {
    return this.listByComposite.has(objectType);
  }

  listUrl(objectType: Composite) {
    return this.listByComposite.get(objectType);
  }

  list(objectType: Composite) {
    const url = this.listUrl(objectType);
    if (url != null) {
      this.router.navigate([url]);
    }
  }

  hasOverview(obj: IObject): boolean {
    return this.overviewByComposite.has(obj.strategy.cls);
  }

  overviewUrl(objectType: Composite) {
    return this.overviewByComposite.get(objectType);
  }

  overview(object: IObject) {
    if (object) {
      const url = this.overviewUrl(object.strategy.cls)?.replace(
        `:id`,
        object.strategy.id
      );
      if (url != null) {
        this.router.navigate([url]);
      }
    }
  }
}
