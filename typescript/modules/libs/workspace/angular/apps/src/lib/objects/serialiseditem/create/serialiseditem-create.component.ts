import { Component, OnDestroy, OnInit, Self, Optional, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Locale, Organisation, Party, Part, InternalOrganisation, Ownership, SerialisedItem, Enumeration, SerialisedItemState } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './serialiseditem-create.component.html',
  providers: [ContextService],
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
    @Self() public allors: ContextService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SerialisedItemCreateComponent>,
    private refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.locales,
            pull.Party({ objectId: this.data.associationId }),
            pull.Ownership({ sorting: [{ roleType: m.Ownership.Name }] }),
            pull.Part({
              name: 'forPart',
              objectId: this.data.associationId,
              include: {
                SerialisedItems: x,
              },
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

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);
        const externalOwner = loaded.object<Party>(m.Party);
        this.owner = externalOwner || internalOrganisation;

        this.part = loaded.object<Part>('forPart');

        this.serialisedItemStates = loaded.collection<SerialisedItemState>(m.SerialisedItemState);
        this.serialisedItemAvailabilities = loaded.collection<Enumeration>(m.Enumeration);
        this.ownerships = loaded.collection<Ownership>(m.Ownership);
        this.locales = loaded.collection<Locale>(m.Locale);

        this.serialisedItem = this.allors.context.create<SerialisedItem>(m.SerialisedItem);
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

      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const pulls = [
        pull.Part({
          object: part,
          include: {
            SerialisedItems: x,
          },
        }),
      ];

      this.allors.context.pull(pulls).subscribe((loaded) => {
        this.selectedPart = loaded.object<Part>(m.Part);
        this.serialisedItem.Name = this.selectedPart.Name;
      });
    } else {
      this.selectedPart = undefined;
    }
  }

  public save(): void {
    this.selectedPart.addSerialisedItem(this.serialisedItem);

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.serialisedItem);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
