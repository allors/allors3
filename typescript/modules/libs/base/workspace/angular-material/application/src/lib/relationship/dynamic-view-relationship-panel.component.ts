import {
  Component,
  OnInit,
  HostBinding,
  Input,
  OnDestroy,
} from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  AllorsViewRelationshipPanelComponent,
  OverviewPageService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import {
  WorkspaceService,
  OnPullService,
} from '@allors/base/workspace/angular/foundation';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import {
  IObject,
  IPullResult,
  OnPull,
  Pull,
} from '@allors/system/workspace/domain';
import { Period } from '@allors/default/workspace/domain';

@Component({
  selector: 'a-mat-dyn-view-rel-panel',
  templateUrl: './dynamic-view-relationship-panel.component.html',
})
export class AllorsMaterialDynamicViewRelationshipPanelComponent
  extends AllorsViewRelationshipPanelComponent
  implements OnPull, OnInit, OnDestroy
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

  @Input()
  display: RoleType;

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
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService,
    private onPullService: OnPullService
  ) {
    super(overviewService, panelService, workspaceService);

    this.panelService.register(this);
    this.onPullService.register(this);
  }

  ngOnInit() {
    this.objectType = this.target.associationType.objectType as Composite;
    this.hasPeriod = this.objectType.supertypes.has(this.m.Period);

    this.title = this.target.pluralName;
  }

  onPrePull(pulls: Pull[], prefix?: string): void {
    const id = this.overviewService.id;

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

  onPostPull(pullResult: IPullResult, prefix?: string): void {
    this.objects = pullResult.collection<IObject>(prefix) ?? [];
    this.updateFilter();

    if (this.hasPeriod) {
      this.description = `${
        this.objects.length - this.filtered.length
      } current and ${
        this.filtered.length
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

  ngOnDestroy(): void {
    this.panelService.unregister(this);
    this.onPullService.unregister(this);
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
