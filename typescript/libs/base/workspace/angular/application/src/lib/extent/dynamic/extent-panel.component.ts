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
  MetaService,
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';
import { AllorsScopedPanelComponent } from '../../scoped/scoped-panel.component';
import { ScopedService } from '../../scoped/scoped.service';
import { PanelService } from '../../panel/panel.service';

export type ExtentSelectType = PropertyType | Path | (PropertyType | Path)[];

export type ExtentInitType = PropertyType;

export type ExtentIncludeType = PropertyType;

@Directive()
export abstract class AllorsDynamicExtentPanelComponent extends AllorsScopedPanelComponent {
  readonly panelKind = 'Extent';

  protected assignedSelect: ExtentSelectType;

  protected assignedTitle: string;

  @Input()
  enabler: () => boolean;

  @Input()
  enabled: boolean;

  @Input()
  creatableFn: () => boolean;

  @Input()
  creatable: boolean;

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

  get propertyType(): PropertyType {
    return this.include
      ? this.include
      : pathLeaf(this.selectAsPaths[0]).propertyType;
  }

  get title() {
    if (this.assignedTitle) {
      return this.assignedTitle;
    }

    const name = this.metaService.pluralName(this.propertyType);
    return humanize(name);
  }

  set title(value: string) {
    this.assignedTitle = value;
  }

  get panelId() {
    // TODO: Koen
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
    refreshService: RefreshService,
    protected metaService: MetaService
  ) {
    super(itemPageService, panelService, sharedPullService, refreshService);
  }
}
