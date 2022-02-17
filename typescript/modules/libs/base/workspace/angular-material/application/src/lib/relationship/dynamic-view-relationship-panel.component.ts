import { Component, OnInit, HostBinding, Input } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  AllorsViewRelationshipPanelComponent,
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
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { Period } from '@allors/default/workspace/domain';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import { M } from '@allors/default/workspace/meta';

@Component({
  selector: 'a-mat-dyn-view-relationship-panel',
  templateUrl: './dynamic-view-relationship-panel.component.html',
})
export class AllorsMaterialDynamicViewRelationshipPanelComponent
  extends AllorsViewRelationshipPanelComponent
  implements SharedPullHandler, OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  @Input()
  anchor: RoleType;

  @Input()
  target: RoleType;

  objectType: Composite;

  get panelId() {
    return `${this.target.name}`;
  }

  m: M;

  title: string;
  description: string;

  hasPeriod: boolean;
  periodSelection: PeriodSelection = PeriodSelection.Current;

  objects: IObject[];
  filtered: IObject[];

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    private diplayService: DisplayService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);

    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.objectType = this.target.associationType.objectType as Composite;
    this.hasPeriod = this.objectType.supertypes.has(this.m.Period);

    this.title = this.target.pluralName;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string): void {
    const id = this.scoped.id;

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: this.objectType,
        predicate: {
          kind: 'Equals',
          propertyType: this.anchor,
          value: id,
        },
      },
      results: [
        {
          name: prefix,
          include: [
            {
              propertyType: this.anchor,
            },
            {
              propertyType: this.target,
            },
          ],
        },
      ],
    };

    pulls.push(pull);
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string): void {
    this.objects = pullResult.collection<IObject>(prefix) ?? [];
    this.updateFilter();

    if (this.hasPeriod) {
      this.description = `${this.filtered.length} current and ${
        this.objects.length - this.filtered.length
      } inactive ${this.target.pluralName.toLowerCase()}`;
    } else {
      this.description = `${
        this.objects.length
      } ${this.target.pluralName.toLowerCase()}`;
    }
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }

  private updateFilter() {
    if (!this.hasPeriod) {
      this.filtered = this.objects;
      return;
    }

    const now = new Date(Date.now());
    switch (this.periodSelection) {
      case PeriodSelection.Current:
        this.filtered = this.objects.filter((v: Period) => {
          if (v.ThroughDate) {
            return v.FromDate < now && v.ThroughDate > now;
          } else {
            return v.FromDate < now;
          }
        });
        break;
      case PeriodSelection.Inactive:
        this.filtered = this.objects.filter((v: Period) => {
          if (v.ThroughDate) {
            return v.FromDate > now || v.ThroughDate < now;
          } else {
            return v.FromDate > now;
          }
        });
        break;
      default:
        this.filtered = this.objects;
        break;
    }
  }
}
