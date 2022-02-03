import { Observable, of, Subscription, switchMap, tap } from 'rxjs';
import { Directive, OnDestroy } from '@angular/core';
import { M } from '@allors/default/workspace/meta';
import {
  IObject,
  IPullResult,
  OnEdit,
  Pull,
} from '@allors/system/workspace/domain';
import {
  Context,
  EditBlocking,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Panel } from '../panel/panel';
import { PanelService } from '../panel/panel.service';
import { AllorsOverviewPanelComponent } from './overview-panel.component';
import { OverviewPageService } from './overview-page.service';

const RESULT_NAME = '_overview_on_edit';

@Directive()
export abstract class AllorsOverviewOnEditPanelComponent
  extends AllorsOverviewPanelComponent
  implements EditBlocking, Panel, OnEdit, OnDestroy
{
  context: Context;

  private subscription: Subscription;

  constructor(
    public overviewService: OverviewPageService,
    public panelService: PanelService,
    public workspaceService: WorkspaceService
  ) {
    super(workspaceService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    panelService.register(this);
  }

  ngAfterViewInit(): void {
    this.subscription = this.overviewService.info$
      .pipe(
        switchMap((info) => {
          this.overviewPageInfo = info;
          const objectId = info.id;

          const context = this.workspaceService.contextBuilder();
          context.name = 'overview-onedit';

          const pulls: Pull[] = [
            {
              objectId,
              results: [
                {
                  name: RESULT_NAME,
                },
              ],
            },
          ];

          this.onPreEdit(objectId, pulls);

          return context.pull(pulls);
        }),
        tap((pullResult) => {
          const object = pullResult.object<IObject>(RESULT_NAME);
          this.onPostEdit(object, pullResult);
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.panelService.unregister(this);

    if (this.subscription) {
      this.subscription?.unsubscribe();
    }
  }

  abstract onPreEdit(objectId: number, pulls: Pull[]);

  abstract onPostEdit(object: IObject, pullResult: IPullResult);

  stopEdit(): Observable<boolean> {
    return of(true);
  }
}
