import { Component, OnInit, HostBinding, Input } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  AllorsViewRelationshipPanelComponent,
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
  ScopedPullHandler,
} from '@allors/system/workspace/domain';
import { Period } from '@allors/default/workspace/domain';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';

@Component({
  selector: 'a-mat-dyn-view-rel-panel',
  templateUrl: './dynamic-view-relationship-panel.component.html',
})
export class AllorsMaterialDynamicViewRelationshipPanelComponent
  extends AllorsViewRelationshipPanelComponent
  implements ScopedPullHandler, OnInit
{
  private assignedAnchor: RoleType;

  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  @Input()
  get anchor(): RoleType {
    if (this.assignedAnchor) {
      return this.assignedAnchor;
    }

    if (this.target) {
      const composite = this.target.associationType.objectType as Composite;
      for (const roleType of composite.roleTypes) {
        if (roleType !== this.target && roleType.relationType.inRelationship) {
          return roleType;
        }
      }
    }

    return null;
  }

  set anchor(value: RoleType) {
    this.assignedAnchor = value;
  }

  @Input()
  target: RoleType;

  objectType: Composite;

  get panelId() {
    return `${this.target.name}`;
  }

  title: string;
  description: string;

  hasPeriod: boolean;
  periodSelection: PeriodSelection = PeriodSelection.Current;

  objects: IObject[];
  filtered: IObject[];

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
    this.objectType = this.target.associationType.objectType as Composite;
    this.hasPeriod = this.objectType.supertypes.has(this.m.Period);

    this.title = this.target.pluralName;
  }

  onPreScopedPull(pulls: Pull[], scope?: string): void {
    const id = this.objectInfo.id;

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
          name: scope,
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

  onPostScopedPull(pullResult: IPullResult, scope?: string): void {
    this.objects = pullResult.collection<IObject>(scope) ?? [];
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
