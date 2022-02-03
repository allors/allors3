import {
  AfterViewInit,
  Directive,
  HostBinding,
  OnDestroy,
} from '@angular/core';
import { Panel, PanelKind, PanelMode } from '../panel/panel';
import { ItemPageInfo, ItemPageService } from './item-page.service';
import { Subscription, tap } from 'rxjs';
import { PanelService } from '../panel/panel.service';
import {
  IPullResult,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import {
  AllorsComponent,
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';

@Directive()
export abstract class AllorsItemPanelComponent
  extends AllorsComponent
  implements Panel, SharedPullHandler, AfterViewInit, OnDestroy
{
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.itemPageInfo.id;
  }

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsObjectType() {
    return this.itemPageInfo?.objectType?.tag;
  }

  m: M;

  abstract panelId: string;

  abstract panelMode: PanelMode;

  abstract panelKind: PanelKind;

  panelEnabled: boolean;

  itemPageInfo: ItemPageInfo;

  private subscription: Subscription;

  constructor(
    public itemPageService: ItemPageService,
    public panelService: PanelService,
    public sharedPullService: SharedPullService,
    public refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super();

    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    panelService.register(this);
    this.sharedPullService.register(this);
  }

  ngAfterViewInit(): void {
    this.subscription = this.itemPageService.info$
      .pipe(
        tap((info) => {
          this.itemPageInfo = info;
          this.refreshService.refresh();
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.panelService.unregister(this);
    this.sharedPullService.unregister(this);

    if (this.subscription) {
      this.subscription?.unsubscribe();
    }
  }

  abstract onPreSharedPull(pulls: Pull[], prefix?: string): void;

  abstract onPostSharedPull(pullResult: IPullResult, prefix?: string): void;
}
