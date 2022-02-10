import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { combineLatest } from 'rxjs';
import { delay, map, switchMap } from 'rxjs/operators';

import { WorkTask } from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsOverviewPageComponent,
  NavigationActivatedRoute,
  NavigationService,
  ObjectService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';

@Component({
  templateUrl: './worktask-overview-page.component.html',
  providers: [
    ObjectService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class WorkTaskOverviewComponent extends AllorsOverviewPageComponent {
  object: WorkTask;

  constructor(
    @Self() objectService: ObjectService,
    @Self() private panelService: PanelService,
    public navigation: NavigationService,
    private titleService: Title,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    private workspaceService: WorkspaceService,
    route: ActivatedRoute
  ) {
    super(objectService, sharedPullService, refreshService, workspaceService);

    this.objectService.objectInfo$ = combineLatest([
      route.url,
      route.queryParams,
    ]).pipe(
      delay(1),
      map(() => new NavigationActivatedRoute(route)),
      switchMap((navRoute) => {
        return this.panelService
          .startEdit(navRoute.panel())
          .pipe(map(() => navRoute));
      }),
      map((navRoute) => {
        return {
          objectType: this.m.Organisation,
          id: navRoute.id(),
        };
      })
    );
  }

  onPreScopedPull(pulls: Pull[], scope?: string) {
    const m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const { pullBuilder: p } = m;

    pulls.push(
      p.WorkTask({
        name: scope,
        objectId: this.objectInfo.id,
      })
    );
  }

  onPostScopedPull(pullResult: IPullResult, scope?: string) {
    this.object = pullResult.object(scope);
    const title = this.objectInfo.objectType.singularName;
    this.titleService.setTitle(title);
  }
}
