import {
  AfterViewInit,
  Directive,
  HostBinding,
  OnDestroy,
} from '@angular/core';
import { Panel, PanelKind, PanelMode } from '../panel/panel';
import { ObjectService } from './object.service';
import { Subscription, tap } from 'rxjs';
import { PanelService } from '../panel/panel.service';
import {
  IPullResult,
  Pull,
  ScopedPullHandler,
} from '@allors/system/workspace/domain';
import {
  AllorsComponent,
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { ObjectInfo } from './object-info';

@Directive()
export abstract class AllorsObjectPanelComponent
  extends AllorsComponent
  implements Panel, ScopedPullHandler, AfterViewInit, OnDestroy
{
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.objectInfo?.id;
  }

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsObjectType() {
    return this.objectInfo?.objectType?.tag;
  }

  m: M;

  abstract panelId: string;

  abstract panelMode: PanelMode;

  abstract panelKind: PanelKind;

  panelEnabled: boolean;

  objectInfo: ObjectInfo;

  private subscription: Subscription;

  constructor(
    public objectService: ObjectService,
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
    this.subscription = this.objectService.objectInfo$
      .pipe(
        tap((info) => {
          this.objectInfo = info;
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

  abstract onPreScopedPull(pulls: Pull[], scope?: string): void;

  abstract onPostScopedPull(pullResult: IPullResult, scope?: string): void;
}
