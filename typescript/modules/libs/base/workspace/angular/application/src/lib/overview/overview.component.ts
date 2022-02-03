import { Directive, HostBinding } from '@angular/core';
import {
  AllorsComponent,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { OverviewPageInfo } from './overview-page.service';

@Directive()
export abstract class AllorsOverviewComponent extends AllorsComponent {
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.overviewPageInfo.id;
  }

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsFromRelationType() {
    return this.overviewPageInfo.objectType?.tag;
  }

  overviewPageInfo: OverviewPageInfo;

  constructor(workspaceService: WorkspaceService) {
    super(workspaceService);
  }
}
