import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Person } from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  PanelService,
  ScopedService,
  AllorsOverviewPageComponent,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Path, Pull } from '@allors/system/workspace/domain';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';
import { PropertyType } from '@allors/system/workspace/meta';

@Component({
  templateUrl: './person-overview-page.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class PersonOverviewPageComponent extends AllorsOverviewPageComponent {
  m: M;

  object: Person;

  contactMechanismTarget: Path;
  serialisedItemTarget: PropertyType[];

  get workEffortContactPerson() {
    return [
      this.m.Party.WorkEffortsWhereCustomer,
      this.m.Person.WorkEffortsWhereContactPerson,
    ];
  }

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    public navigation: NavigationService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    route: ActivatedRoute,
    workspaceService: WorkspaceService
  ) {
    super(
      scopedService,
      panelService,
      sharedPullService,
      refreshService,
      route,
      workspaceService
    );
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    const { m } = this;
    const { pathBuilder: p } = this.m;

    this.contactMechanismTarget = p.Party({
      PartyContactMechanismsWhereParty: { ContactMechanism: {} },
    });

    this.serialisedItemTarget = [
      m.Party.SerialisedItemsWhereOwnedBy,
      m.Party.SerialisedItemsWhereRentedBy,
    ];
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.Person({
        name: prefix,
        objectId: id,
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.object = loaded.object(prefix);
  }
}
