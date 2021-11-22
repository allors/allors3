import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { InternalOrganisation, Organisation, CustomerRelationship, Party } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './customerrelationship-edit.component.html',
  providers: [ContextService],
})
export class CustomerRelationshipEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  partyRelationship: CustomerRelationship;
  internalOrganisation: InternalOrganisation;
  party: Party;
  title: string;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<CustomerRelationshipEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [this.fetcher.internalOrganisation];

          if (!isCreate) {
            pulls.push(
              pull.CustomerRelationship({
                objectId: this.data.id,
                include: {
                  InternalOrganisation: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Party({
                objectId: this.data.associationId,
              })
            );
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.party = loaded.object<Party>(m.Party);

        if (isCreate) {
          this.title = 'Add Customer Relationship';

          this.partyRelationship = this.allors.context.create<CustomerRelationship>(m.CustomerRelationship);
          this.partyRelationship.FromDate = new Date();
          this.partyRelationship.Customer = this.party;
          this.partyRelationship.InternalOrganisation = this.internalOrganisation;
        } else {
          this.partyRelationship = loaded.object<CustomerRelationship>(m.CustomerRelationship);

          if (this.partyRelationship.canWriteFromDate) {
            this.title = 'Edit Customer Relationship';
          } else {
            this.title = 'View Customer Relationship';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.partyRelationship);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
