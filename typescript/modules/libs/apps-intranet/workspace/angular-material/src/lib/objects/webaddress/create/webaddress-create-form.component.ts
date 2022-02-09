import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
  WebAddress,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './webaddress-create-form.component.html',
  providers: [ContextService],
})
export class WebAddressCreateFormComponent
  extends AllorsFormComponent<WebAddress>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  public title = 'Add Web Address';

  contactMechanism: ElectronicAddress;

  private subscription: Subscription;
  party: Party;
  partyContactMechanism: PartyContactMechanism;
  contactMechanismPurposes: Enumeration[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.Party({
              objectId: this.data.associationId,
              include: { PartyContactMechanisms: x },
            }),
            pull.ContactMechanismPurpose({
              predicate: {
                kind: 'Equals',
                propertyType: m.ContactMechanismPurpose.IsActive,
                value: true,
              },
              sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.party = loaded.object<Party>(m.Party);
        this.contactMechanismPurposes =
          loaded.collection<ContactMechanismPurpose>(m.ContactMechanismPurpose);

        this.contactMechanism = this.allors.context.create<WebAddress>(
          m.WebAddress
        );

        this.partyContactMechanism =
          this.allors.context.create<PartyContactMechanism>(
            m.PartyContactMechanism
          );
        this.partyContactMechanism.UseAsDefault = true;
        this.partyContactMechanism.ContactMechanism = this.contactMechanism;

        this.party.addPartyContactMechanism(this.partyContactMechanism);
      });
  }
}
