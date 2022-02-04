import { Component, OnDestroy, QueryList, ViewChildren } from '@angular/core';
import {
  PanelService,
  ObjectService,
  AllorsEditDetailPanelComponent,
} from '@allors/base/workspace/angular/application';
import {
  AllorsForm,
  angularForms,
  RefreshService,
  SharedPullService,
  TemplateHostDirective,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { map, Subscription } from 'rxjs';
import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

@Component({
  selector: 'a-mat-dyn-edit-detail-panel',
  templateUrl: './dynamic-edit-detail-panel.component.html',
})
export class AllorsMaterialDynamicEditDetailPanelComponent
  extends AllorsEditDetailPanelComponent
  implements OnDestroy
{
  @ViewChildren(TemplateHostDirective)
  templateHosts!: QueryList<TemplateHostDirective>;

  title: string;

  form: AllorsForm;

  private templateSubscription: Subscription;
  private cancelledSubscription: Subscription;
  private savedSubscription: Subscription;

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }

  onPreSharedPull(pulls: Pull[], scope?: string): void {
    pulls.push({ objectId: this.objectInfo.id, results: [{ name: scope }] });

    this.subscribeTemplate();
  }

  onPostSharedPull(pullResult: IPullResult, scope?: string): void {
    const object = pullResult.object<IObject>(scope);
    this.title = `Edit ${object}`;
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();

    this.templateSubscription?.unsubscribe();
    this.cancelledSubscription?.unsubscribe();
    this.savedSubscription?.unsubscribe();
  }

  private subscribeTemplate() {
    this.templateSubscription?.unsubscribe();
    this.templateSubscription = this.templateHosts.changes.subscribe(() => {
      const templateHost = this.templateHosts.first;

      this.cancelledSubscription?.unsubscribe();
      this.savedSubscription?.unsubscribe();

      if (!templateHost) {
        return;
      }

      const viewContainerRef = templateHost.viewContainerRef;
      viewContainerRef.clear();

      const componentRef = viewContainerRef.createComponent<AllorsForm>(
        angularForms(this.objectInfo.objectType).edit
      );

      this.form = componentRef.instance;
      this.form.edit(this.objectInfo.id);

      this.cancelledSubscription = this.form.cancelled
        .pipe(
          map(() => {
            this.panelService.stopEdit().subscribe();
          })
        )
        .subscribe();

      this.savedSubscription = this.form.saved
        .pipe(
          map(() => {
            this.panelService.stopEdit().subscribe();
          })
        )
        .subscribe();
    });
  }
}
