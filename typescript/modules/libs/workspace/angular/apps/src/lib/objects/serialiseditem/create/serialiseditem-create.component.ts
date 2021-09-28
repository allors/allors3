import { Component, OnDestroy, OnInit, Self, Optional, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { SessionService, MetaService, RefreshService } from '@allors/angular/services/core';
import { PullRequest } from '@allors/protocol/system';
import { ObjectData, SaveService } from '@allors/angular/material/services/core';
import {
  Organisation,
  Part,
  Enumeration,
  Party,
  SerialisedItem,
  Ownership,
  SerialisedItemState,
} from '@allors/domain/generated';
import { Equals, Sort } from '@allors/data/system';
import { FetcherService, InternalOrganisationId, Filters } from '@allors/angular/base';
import { IObject, IObject } from '@allors/domain/system';
import { Meta } from '@allors/meta/generated';
import { TestScope, SearchFactory } from '@allors/angular/core';


@Component({
  templateUrl: './serialiseditem-create.component.html',
  providers: [SessionService]
})
export class SerialisedItemCreateComponent extends TestScope implements OnInit, OnDestroy {

  readonly m: M;
  serialisedItem: SerialisedItem;

  public title = 'Add Serialised Asset';

  locales: Locale[];
  ownerships: Ownership[];
  organisations: Organisation[];
  organisationFilter: SearchFactory;
  serialisedItemStates: SerialisedItemState[];
  owner: Party;
  part: Part;
  itemPart: Part;
  selectedPart: Part;

  private subscription: Subscription;
  serialisedItemAvailabilities: Enumeration[];
  serialisedgoodsFilter: SearchFactory;
  partiesFilter: SearchFactory;

  constructor(
    @Self() public allors: SessionService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SerialisedItemCreateComponent>,
    
    private refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId,
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const m = this.allors.workspace.configuration.metaPopulation as M; const { pullBuilder: pull } = m; const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {

          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.locales,
            pull.Party({ object: this.data.associationId }),
            pull.Ownership({ sorting: [{ roleType: m.Ownership.Name }] }),
            pull.Part({
              name: 'forPart',
              object: this.data.associationId,
              include: {
                SerialisedItems: x
              }
            }),
            pull.SerialisedItemState({
              predicate: { kind: 'Equals', propertyType: m.SerialisedItemState.IsActive, value: true },
              sorting: [{ roleType: m.SerialisedInventoryItemState.Name }],
            }),
            pull.SerialisedItemAvailability({
              predicate: { kind: 'Equals', propertyType: m.SerialisedItemAvailability.IsActive, value: true },
              sorting: [{ roleType: m.SerialisedItemAvailability.Name }],
            }),
          ];

          this.partiesFilter = Filters.partiesFilter(m);
          this.serialisedgoodsFilter = Filters.serialisedgoodsFilter(m);

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {

        this.allors.session.reset();

        const internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);
        const externalOwner = loaded.object<Party>(m.Party);
        this.owner = externalOwner || internalOrganisation;

        this.part = loaded.object<forPart>(m.forPart);

        this.serialisedItemStates = loaded.collection<SerialisedItemState>(m.SerialisedItemState);
        this.serialisedItemAvailabilities = loaded.collection<Enumeration>(m.Enumeration);
        this.ownerships = loaded.collection<Ownership>(m.Ownership);
        this.locales = loaded.collection<Locale>(m.Locale);

        this.serialisedItem = this.allors.session.create<SerialisedItem>(m.SerialisedItem);
        this.serialisedItem.AvailableForSale = false;
        this.serialisedItem.OwnedBy = this.owner;

        if (this.part) {
          this.partSelected(this.part);
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public partSelected(obj: IObject): void {
    if (obj) {
      const part = obj as Part;
      this.selectedPart = part;
      this.serialisedItem.Name = part.Name;

      const m = this.m; const { pullBuilder: pull } = m; const x = {};

      const pulls = [
        pull.Part(
          {
            object: part,
            include: {
              SerialisedItems: x
            }
          }
        ),
      ];

      this.allors.context
        .load(new PullRequest({ pulls }))
        .subscribe((loaded) => {
          this.selectedPart = loaded.object<Part>(m.Part);
          this.serialisedItem.Name = this.selectedPart.Name;
        });

    } else {
      this.selectedPart = undefined;
    }
  }

  public save(): void {

    this.selectedPart.AddSerialisedItem(this.serialisedItem);

    this.allors.context
      .save()
      .subscribe(() => {
        const data: IObject = {
          id: this.serialisedItem.id,
          objectType: this.serialisedItem.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }
}
