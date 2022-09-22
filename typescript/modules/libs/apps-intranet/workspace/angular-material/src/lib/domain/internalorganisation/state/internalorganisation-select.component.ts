import { Component, Self, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { Organisation } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { M } from '@allors/default/workspace/meta';

@Component({
  selector: 'internalorganisation-select',
  templateUrl: './internalorganisation-select.component.html',
  providers: [ContextService],
})
export class SelectInternalOrganisationComponent implements OnInit, OnDestroy {
  m: any;
  public get internalOrganisation() {
    const internalOrganisation = this.internalOrganisations?.find(
      (v) => v.strategy.id === this.internalOrganisationId.value
    );
    return internalOrganisation;
  }

  public set internalOrganisation(value: Organisation) {
    this.internalOrganisationId.value = value.id;
  }

  public internalOrganisations: Organisation[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    const pulls = [
      pull.Organisation({
        predicate: {
          kind: 'Equals',
          propertyType: m.Organisation.IsInternalOrganisation,
          value: true,
        },
        sorting: [{ roleType: m.Organisation.DisplayName }],
      }),
    ];

    this.subscription = this.allors.context.pull(pulls).subscribe((loaded) => {
      const internalOrganisations = loaded.collection<Organisation>(
        m.Organisation
      );

      this.internalOrganisations = internalOrganisations?.filter(
        (v) => v.canExecuteShowInMenu
      );
    });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
