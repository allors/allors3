import { Directive, Input } from '@angular/core';

import { Composite, PropertyType } from '@allors/system/workspace/meta';
import { isPath, Path, pathObjectType } from '@allors/system/workspace/domain';
import {
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';
import { AllorsScopedPanelComponent } from '../scoped/scoped-panel.component';
import { ScopedService } from '../scoped/scoped.service';
import { PanelService } from '../panel/panel.service';

export type ExtentSelectType = PropertyType | Path;

export type ExtentInitType = PropertyType;

export type ExtentIncludeType = PropertyType;

@Directive()
export abstract class AllorsExtentPanelComponent extends AllorsScopedPanelComponent {
  readonly panelKind = 'Extent';

  @Input()
  select: ExtentSelectType;

  @Input()
  init: ExtentInitType;

  @Input()
  include: ExtentIncludeType;

  get panelId() {
    return this.include.name;
  }

  get objectType(): Composite {
    if (isPath(this.select)) {
      return pathObjectType(this.select);
    } else {
      return this.select.objectType as Composite;
    }
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
