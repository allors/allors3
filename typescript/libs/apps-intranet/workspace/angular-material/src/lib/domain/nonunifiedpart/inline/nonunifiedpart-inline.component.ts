import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import {
  NonUnifiedPart,
  InternalOrganisation,
  InventoryItemKind,
  UnitOfMeasure,
  PartCategory,
} from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  selector: 'nonunifiedpart-inline',
  templateUrl: './nonunifiedpart-inline.component.html',
})
export class NonUnifiedPartInlineComponent implements OnInit, OnDestroy {
  @Output() public saved: EventEmitter<NonUnifiedPart> =
    new EventEmitter<NonUnifiedPart>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  public m: M;

  public object: NonUnifiedPart;
  internalOrganisation: InternalOrganisation;
  inventoryItemKinds: InventoryItemKind[];
  unitsOfMeasure: UnitOfMeasure[];
  categories: PartCategory[];
  selectedCategories: PartCategory[] = [];

  constructor(
    private allors: ContextService,

    private fetcher: FetcherService
  ) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    const pulls = [
      this.fetcher.internalOrganisation,
      p.PartCategory({
        sorting: [{ roleType: m.PartCategory.Name }],
      }),
      p.UnitOfMeasure({
        sorting: [{ roleType: this.m.UnitOfMeasure.Name }],
      }),
      p.InventoryItemKind({
        sorting: [{ roleType: this.m.InventoryItemKind.Name }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
      this.categories = loaded.collection<PartCategory>(this.m.PartCategory);

      this.inventoryItemKinds = loaded.collection<InventoryItemKind>(
        this.m.InventoryItemKind
      );
      const nonSerialised = this.inventoryItemKinds?.find(
        (v) => v.UniqueId === 'eaa6c331-0dd9-4bb1-8245-12a673304468'
      );

      this.unitsOfMeasure = loaded.collection<UnitOfMeasure>(
        this.m.UnitOfMeasure
      );
      const piece = this.unitsOfMeasure?.find(
        (v) => v.UniqueId === 'f4bbdb52-3441-4768-92d4-729c6c5d6f1b'
      );

      this.object = this.allors.context.create<NonUnifiedPart>(
        m.NonUnifiedPart
      );
      this.object.UnitOfMeasure = piece;
      this.object.InventoryItemKind = nonSerialised;
      this.object.DefaultFacility =
        this.internalOrganisation.FacilitiesWhereOwner[0];
    });
  }

  public categorySelected(categories: PartCategory[]): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    let pulls = [];

    categories.forEach((category: PartCategory) => {
      pulls = [
        ...pulls,
        pull.PartCategory({
          object: category,
          include: {
            Parts: x,
          },
        }),
      ];
    });

    this.allors.context.pull(pulls).subscribe((pullResult) => {});
  }

  public ngOnDestroy(): void {
    if (this.object) {
      this.object.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.selectedCategories.forEach((category: PartCategory) => {
      category.addPart(this.object);
    });

    this.saved.emit(this.object);
    this.object = undefined;
  }
}
