import { Component, Self } from '@angular/core';
import { isBefore, isAfter } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { Part, BasePrice, PriceComponent, SupplierOffering, ProductIdentificationType } from '@allors/workspace/domain/default';
import { NavigationService, PanelService } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { SortDirection } from '@allors/workspace/domain/system';

@Component({
  
  selector: 'nonunifiedpart-overview-summary',
  templateUrl: './nonunifiedpart-overview-summary.component.html',
  providers: [PanelService],
})
export class NonUnifiedPartOverviewSummaryComponent {
  m: M;

  part: Part;
  serialised: boolean;
  suppliers: string;
  sellingPrice: BasePrice;
  currentPricecomponents: PriceComponent[] = [];
  inactivePricecomponents: PriceComponent[] = [];
  allPricecomponents: PriceComponent[] = [];
  allSupplierOfferings: SupplierOffering[];
  currentSupplierOfferings: SupplierOffering[];
  inactiveSupplierOfferings: SupplierOffering[];
  partnumber: string[];

  constructor(@Self() public panel: PanelService, public workspaceService: WorkspaceService, public navigation: NavigationService) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    panel.name = 'summary';

    const partPullName = `${panel.name}_${this.m.Part.tag}`;
    const priceComponentPullName = `${panel.name}_${this.m.PriceComponent.tag}`;
    const supplierOfferingsPullName = `${panel.name}_${this.m.SupplierOffering.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.PriceComponent({
          name: priceComponentPullName,
          predicate: { kind: 'Equals', propertyType: m.PriceComponent.Part, value: id },
          include: {
            Part: x,
            Currency: x,
          },
          sorting: [{ roleType: m.PriceComponent.FromDate, sortDirection: SortDirection.Descending }],
        }),
        pull.Part({
          name: partPullName,
          objectId: id,
          include: {
            ProductIdentifications: {
              ProductIdentificationType: x,
            },
            ProductType: x,
            InventoryItemKind: x,
            ManufacturedBy: x,
            SuppliedBy: x,
            SerialisedItems: {
              PrimaryPhoto: x,
              SerialisedItemState: x,
              OwnedBy: x,
            },
            Brand: x,
            Model: x,
          },
        }),
        pull.Part({
          name: supplierOfferingsPullName,
          objectId: id,
          select: {
            SupplierOfferingsWherePart: {
              include: {
                Currency: x,
              },
            },
          },
        }),
        pull.ProductIdentificationType({})
      );
    };

    panel.onPulled = (loaded) => {
      this.part = loaded.object<Part>(partPullName);
      this.serialised = this.part.InventoryItemKind.UniqueId === '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

      this.allPricecomponents = loaded.collection<PriceComponent>(priceComponentPullName);
      this.currentPricecomponents = this.allPricecomponents?.filter((v) => isBefore(new Date(v.FromDate), new Date()) && (v.ThroughDate == null || isAfter(new Date(v.ThroughDate), new Date())));
      this.inactivePricecomponents = this.allPricecomponents?.filter((v) => isAfter(new Date(v.FromDate), new Date()) || (v.ThroughDate != null && isBefore(new Date(v.ThroughDate), new Date())));

      this.allSupplierOfferings = loaded.collection<SupplierOffering>(supplierOfferingsPullName);
      this.currentSupplierOfferings = this.allSupplierOfferings?.filter((v) => isBefore(new Date(v.FromDate), new Date()) && (v.ThroughDate == null || isAfter(new Date(v.ThroughDate), new Date())));
      this.inactiveSupplierOfferings = this.allSupplierOfferings?.filter((v) => isAfter(new Date(v.FromDate), new Date()) || (v.ThroughDate != null && isBefore(new Date(v.ThroughDate), new Date())));

      const goodIdentificationTypes = loaded.collection<ProductIdentificationType>(this.m.ProductIdentificationType);
      const partNumberType = goodIdentificationTypes?.find((v) => v.UniqueId === '5735191a-cdc4-4563-96ef-dddc7b969ca6');
      this.partnumber = this.part.ProductIdentifications?.filter((v) => v.ProductIdentificationType === partNumberType)?.map((w) => w.Identification);

      if (this.part.SuppliedBy.length > 0) {
        this.suppliers = this.part.SuppliedBy?.map((v) => v.DisplayName)?.reduce((acc: string, cur: string) => acc + ', ' + cur);
      }
    };
  }
}
