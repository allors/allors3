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
  SerialisedItem,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './serialiseditem-create-form.component.html',
  providers: [ContextService],
})
export class SerialisedItemCreateFormComponent
  extends AllorsFormComponent<SerialisedItem>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
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
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
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
              predicate: {
                kind: 'Equals',
                propertyType: m.SerialisedItemState.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.SerialisedInventoryItemState.Name }],
            }),
            pull.SerialisedItemAvailability({
              predicate: {
                kind: 'Equals',
                propertyType: m.SerialisedItemAvailability.IsActive,
                value: true,
              },
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

        const internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        const externalOwner = loaded.object<Party>(m.Party);
        this.owner = externalOwner || internalOrganisation;

        this.part = loaded.object<Part>('forPart');

        this.serialisedItemStates = loaded.collection<SerialisedItemState>(
          m.SerialisedItemState
        );
        this.serialisedItemAvailabilities =
          loaded.collection<SerialisedItemAvailability>(
            m.SerialisedItemAvailability
          );
        this.ownerships = loaded.collection<Ownership>(m.Ownership);
        this.locales = this.fetcher.getAdditionalLocales(loaded);

        this.serialisedItem = this.allors.context.create<SerialisedItem>(
          m.SerialisedItem
        );
        this.serialisedItem.AvailableForSale = false;
        this.serialisedItem.OwnedBy = this.owner;

        if (this.part) {
          this.partSelected(this.part);
        }
      });
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

  public override save(): void {
    this.selectedPart.addSerialisedItem(this.serialisedItem);

    super.save();
  }
}
