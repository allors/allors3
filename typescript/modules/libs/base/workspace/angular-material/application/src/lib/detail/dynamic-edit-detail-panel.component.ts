import {
  AfterViewInit,
  Component,
  OnDestroy,
  QueryList,
  ViewChildren,
} from '@angular/core';
import {
  PanelService,
  ItemPageService,
  AllorsItemEditDetailPanelComponent,
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

@Component({
  selector: 'a-mat-dyn-edit-detail-panel',
  templateUrl: './dynamic-edit-detail-panel.component.html',
})
export class AllorsMaterialDynamicEditDetailPanelComponent
  extends AllorsItemEditDetailPanelComponent
  implements AfterViewInit, OnDestroy
{
  @ViewChildren(TemplateHostDirective)
  templateHosts!: QueryList<TemplateHostDirective>;

  form: AllorsForm;

  private cancelledSubscription: Subscription;
  private savedSubscription: Subscription;

  constructor(
    itemPageService: ItemPageService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(
      itemPageService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }

  onPreSharedPull(): void {
    this.templateHosts.changes.subscribe(() => {
      const templateHost = this.templateHosts.first;

      this.cancelledSubscription?.unsubscribe();
      this.savedSubscription?.unsubscribe();

      if (!templateHost) {
        return;
      }

      const viewContainerRef = templateHost.viewContainerRef;
      viewContainerRef.clear();

      const componentRef = viewContainerRef.createComponent<AllorsForm>(
        angularForms(this.itemPageInfo.objectType).edit
      );

      this.form = componentRef.instance;
      this.form.edit(this.itemPageInfo.id);

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

  onPostSharedPull(): void {}

  override ngOnDestroy(): void {
    super.ngOnDestroy();

    this.cancelledSubscription?.unsubscribe();
    this.savedSubscription?.unsubscribe();
  }
}
