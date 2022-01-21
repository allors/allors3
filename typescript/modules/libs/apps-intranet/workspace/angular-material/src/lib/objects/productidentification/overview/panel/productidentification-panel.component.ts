import { Component, OnInit, Self, HostBinding, Input } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import { ProductIdentification } from '@allors/workspace/domain/default';
import {
  Action,
  DeleteService,
  EditService,
  ObjectData,
  ObjectService,
  PanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/workspace/angular/base';
import { RoleType } from '@allors/system/workspace/meta';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: ProductIdentification;
  type: string;
  identification: string;
}

@Component({
  selector: 'productidentification-panel',
  templateUrl: './productIdentification-panel.component.html',
  providers: [PanelService],
})
export class ProductIdentificationsPanelComponent implements OnInit {
  @Input() roleType: RoleType;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: ProductIdentification[];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
      associationRoleType: this.roleType,
    };
  }
  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public objectService: ObjectService,
    public refreshService: RefreshService,
    public editService: EditService,
    public deleteService: DeleteService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;
    const { treeBuilder: tree } = m;
    const x = {};

    this.panel.name = 'productidentification';
    this.panel.title = 'Product Identification';
    this.panel.icon = 'fingerprint';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'type', sort },
        { name: 'identification', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.ProductIdentification.tag}`;

    this.panel.onPull = (pulls) => {
      const { id, objectType } = this.panel.manager;

      pulls.push({
        objectType: objectType,
        objectId: id,
        results: [
          {
            name: pullName,
            select: {
              propertyType: this.roleType,
              include: tree.ProductIdentification({
                ProductIdentificationType: x,
              }),
            },
          },
        ],
      });

      this.panel.onPulled = (loaded) => {
        this.objects = loaded.collection<ProductIdentification>(pullName);
        this.table.total =
          (loaded.value(`${pullName}_total`) as number) ??
          this.objects?.length ??
          0;
        this.table.data = this.objects?.map((v) => {
          return {
            object: v,
            type:
              v.ProductIdentificationType && v.ProductIdentificationType.Name,
            identification: v.Identification,
          } as Row;
        });
      };
    };
  }
}
