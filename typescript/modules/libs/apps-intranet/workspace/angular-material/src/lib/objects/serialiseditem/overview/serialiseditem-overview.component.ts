import {
  Component,
  Self,
  AfterViewInit,
  OnDestroy,
  Injector,
} from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

import { M } from '@allors/default/workspace/meta';
import { Party, Part, SerialisedItem } from '@allors/default/workspace/domain';
import {
  NavigationActivatedRoute,
  NavigationService,
  OldPanelManagerService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  ContextService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './serialiseditem-overview.component.html',
  providers: [OldPanelManagerService, ContextService],
})
export class SerialisedItemOverviewComponent
  implements AfterViewInit, OnDestroy
{
  readonly m: M;
  title = 'Asset';

  serialisedItem: SerialisedItem;

  subscription: Subscription;
  part: Part;
  owner: Party;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panelManager: OldPanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    titleService.setTitle(this.title);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(
      this.route.url,
      this.route.queryParams,
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.objectType = m.SerialisedItem;
          this.panelManager.id = navRoute.id();
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.SerialisedItem({
              objectId: this.panelManager.id,
              include: {
                OwnedBy: x,
              },
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();
        this.panelManager.onPulled(loaded);

        this.serialisedItem = loaded.object<SerialisedItem>(m.SerialisedItem);
        this.owner = this.serialisedItem.OwnedBy;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
