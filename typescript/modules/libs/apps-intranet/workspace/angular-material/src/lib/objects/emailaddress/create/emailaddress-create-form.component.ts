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
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './emailaddress-create-form.component.html',
  providers: [ContextService],
})
export class EmailAddressCreateFormComponent implements OnInit, OnDestroy {
  readonly m: M;

  contactMechanism: ElectronicAddress;
  contactMechanismTypes: Enumeration[];

  public title = 'Add Email Address';

  private subscription: Subscription;
  partyContactMechanism: PartyContactMechanism;
  party: Party;
  contactMechanismPurposes: Enumeration[];

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<EmailAddressCreateFormComponent>,

    public refreshService: RefreshService,
    private errorService: ErrorService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
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
              include: {
                PartyContactMechanisms: x,
              },
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

        this.contactMechanism = this.allors.context.create<EmailAddress>(
          m.EmailAddress
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

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.contactMechanism);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
