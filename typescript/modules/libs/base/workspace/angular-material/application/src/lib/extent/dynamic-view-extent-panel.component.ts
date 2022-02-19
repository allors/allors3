import { Component, OnInit, HostBinding, Input } from '@angular/core';
import { Composite, PropertyType } from '@allors/system/workspace/meta';
import {
  AllorsViewExtentPanelComponent,
  ScopedService,
  PanelService,
  ExtentSelectType,
  ExtentInitType,
  ExtentIncludeType,
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
  Path,
  pathObjectType,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { Period } from '@allors/default/workspace/domain';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import { M } from '@allors/default/workspace/meta';

@Component({
  selector: 'a-mat-dyn-view-extent-panel',
  templateUrl: './dynamic-view-extent-panel.component.html',
})
export class AllorsMaterialDynamicViewLinkPanelComponent
  extends AllorsViewExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
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
    this.hasPeriod = this.objectType.supertypes.has(this.m.Period);

    this.title = this.include?.pluralName ?? this.objectType.pluralName;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string): void {
    const id = this.scoped.id;

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: this.objectType,
        predicate: {
          kind: 'Equals',
          propertyType: this.init,
          value: id,
        },
      },
      results: [
        {
          name: prefix,
          include: [
            {
              propertyType: this.init,
            },
            {
              propertyType: this.include,
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
      } inactive ${this.include.pluralName.toLowerCase()}`;
    } else {
      this.description = `${
        this.objects.length
      } ${this.include.pluralName.toLowerCase()}`;
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
