import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  WorkTask,
  Good,
  InternalOrganisation,
  NonUnifiedGood,
  Part,
  PriceComponent,
  Brand,
  Model,
  Locale,
  Carrier,
  SerialisedItemCharacteristicType,
  WorkTask,
  ContactMechanism,
  Person,
  Organisation,
  PartyContactMechanism,
  OrganisationContactRelationship,
  Catalogue,
  Singleton,
  ProductCategory,
  Scope,
  CommunicationEvent,
  WorkEffortState,
  Priority,
  WorkEffortPurpose,
  WorkEffortPartyAssignment,
  CustomerRelationship,
  Party,
} from '@allors/workspace/domain/default';
import {
  Action,
  DeleteService,
  EditService,
  Filter,
  FilterDefinition,
  MediaService,
  NavigationService,
  ObjectData,
  ObjectService,
  OverviewService,
  PanelService,
  RefreshService,
  SaveService,
  SearchFactory,
  Sorter,
  Table,
  TableRow,
  TestScope,
} from '@allors/workspace/angular/base';
import { SessionService, WorkspaceService } from '@allors/workspace/angular/core';
import { And } from '@allors/workspace/domain/system';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './customerrelationship-edit.component.html',
  providers: [SessionService],
})
export class CustomerRelationshipEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  partyRelationship: CustomerRelationship;
  internalOrganisation: Organisation;
  party: Party;
  title: string;

  private subscription: Subscription;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<CustomerRelationshipEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

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

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);
        this.party = loaded.object<Party>(m.Party);

        if (isCreate) {
          this.title = 'Add Customer Relationship';

          this.partyRelationship = this.allors.session.create<CustomerRelationship>(m.CustomerRelationship);
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
    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.dialogRef.close(this.partyRelationship);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
