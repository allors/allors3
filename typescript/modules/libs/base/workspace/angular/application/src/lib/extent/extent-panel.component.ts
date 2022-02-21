import { Directive, Input } from '@angular/core';

import {
  AssociationType,
  Composite,
  humanize,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  isPath,
  Path,
  pathLeaf,
  pathObjectType,
} from '@allors/system/workspace/domain';
import {
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';
import { AllorsScopedPanelComponent } from '../scoped/scoped-panel.component';
import { ScopedService } from '../scoped/scoped.service';
import { PanelService } from '../panel/panel.service';

export type ExtentSelectType = PropertyType | Path | (PropertyType | Path)[];

export type ExtentInitType = PropertyType;

export type ExtentIncludeType = PropertyType;

@Directive()
export abstract class AllorsExtentPanelComponent extends AllorsScopedPanelComponent {
  readonly panelKind = 'Extent';

  protected assignedSelect: ExtentSelectType;

  protected assignedTitle: string;

  @Input()
  get select(): ExtentSelectType {
    if (this.assignedSelect) {
      return this.assignedSelect;
    } else if (this.init) {
      if (this.init.isRoleType) {
        return (this.init as RoleType).associationType;
      } else {
        return (this.init as AssociationType).roleType;
      }
    }

    return null;
  }

  set select(value: ExtentSelectType) {
    this.assignedSelect = value;
  }

  protected get selectAsPaths(): Path[] {
    if (this.select == null) {
      return [];
    }

    if (Array.isArray(this.select)) {
      return this.select.map((v) =>
        isPath(v) ? v : ({ propertyType: v } as Path)
      );
    } else {
      if (isPath(this.select)) {
        return [this.select];
      } else {
        return [{ propertyType: this.select }];
      }
    }
  }

  @Input()
  init: ExtentInitType;

  @Input()
  include: ExtentIncludeType;

  get title() {
    if (this.assignedTitle) {
      return this.assignedTitle;
    }

    let title: string;
    if (this.include) {
      title = this.include.pluralName;
    } else {
      title = pathLeaf(this.selectAsPaths[0]).propertyType.pluralName;
    }

    return humanize(title);
  }

  set title(value: string) {
    this.assignedTitle = value;
  }

  get panelId() {
    // TODO:
    return this.objectType?.tag;
  }

  get objectType(): Composite {
    const path = this.selectAsPaths[0];
    const objecType = pathObjectType(path);
    return objecType;
  }

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, sharedPullService, refreshService);
  }
}
