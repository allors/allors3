import { Component, OnInit, HostBinding, Input } from '@angular/core';
import {
  Composite,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  AllorsViewObjectPanelComponent,
  ScopedService,
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
  leafPath,
  Path,
  Pull,
  SharedPullHandler,
  toNode,
} from '@allors/system/workspace/domain';

@Component({
  selector: 'a-mat-dyn-view-object-panel',
  templateUrl: './dynamic-view-object-panel.component.html',
})
export class AllorsMaterialDynamicViewObjectPanelComponent
  extends AllorsViewObjectPanelComponent
  implements SharedPullHandler, OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get panelId() {
    return `${this.tag}`;
  }

  @Input()
  anchor: PropertyType | PropertyType[];

  @Input()
  target: PropertyType | Path | (PropertyType | Path)[];

  leaf: Path;

  objectType: Composite;
  tag: string;

  title: string;
  description: string;

  objects: IObject[];

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    private diplayService: DisplayService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
  }

  ngOnInit() {
    // this.leaf = leafPath(this.target);
    // this.objectType = this.leaf.propertyType.objectType as Composite;
    // this.title = this.objectType.pluralName;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string): void {
    // const id = this.scoped.id;
    // const pull: Pull = {
    //   extent: {
    //     kind: 'Filter',
    //     objectType: this.objectType,
    //     predicate: {
    //       kind: 'Equals',
    //       propertyType: this.anchor,
    //       value: id,
    //     },
    //   },
    //   results: [
    //     {
    //       name: prefix,
    //       include: [toNode(this.target)],
    //     },
    //   ],
    // };
    // pulls.push(pull);
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string): void {
    this.objects = pullResult.collection<IObject>(prefix) ?? [];
    this.description = `${
      this.objects.length
    } ${this.objectType.pluralName.toLowerCase()}`;
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }
}
