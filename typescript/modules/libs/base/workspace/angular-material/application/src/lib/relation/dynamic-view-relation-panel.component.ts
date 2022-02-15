import { Component, OnInit, HostBinding, Input } from '@angular/core';
import {
  AssociationType,
  Composite,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  AllorsViewRelationPanelComponent,
  ObjectService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import {
  WorkspaceService,
  SharedPullService,
  RefreshService,
  DisplayService,
} from '@allors/base/workspace/angular/foundation';
import {
  IObject,
  IPullResult,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';

@Component({
  selector: 'a-mat-dyn-view-relation-panel',
  templateUrl: './dynamic-view-relation-panel.component.html',
})
export class AllorsMaterialDynamicViewRelationPanelComponent
  extends AllorsViewRelationPanelComponent
  implements SharedPullHandler, OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  @Input()
  propertyType: PropertyType;

  objectType: Composite;

  get panelId() {
    return `${this.propertyType.name}`;
  }

  title: string;
  description: string;

  object: IObject;
  properties: IObject[];

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    private diplayService: DisplayService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }

  ngOnInit() {
    this.objectType = this.propertyType.objectType as Composite;
    this.title = this.propertyType.pluralName;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string): void {
    const id = this.objectInfo.id;

    const pull: Pull = {
      objectId: id,
      results: [
        {
          name: prefix,
          include: [
            {
              propertyType: this.propertyType,
            },
          ],
        },
      ],
    };

    pulls.push(pull);
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string): void {
    this.object = pullResult.object<IObject>(prefix);

    if (this.propertyType.isAssociationType) {
      this.properties = this.object.strategy.getCompositesAssociation(
        this.propertyType as AssociationType
      ) as IObject[];
    } else {
      this.properties = this.object.strategy.getCompositesRole(
        this.propertyType as RoleType
      ) as IObject[];
    }

    this.description = `${
      this.properties.length
    } ${this.propertyType.pluralName.toLowerCase()}`;
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }
}
