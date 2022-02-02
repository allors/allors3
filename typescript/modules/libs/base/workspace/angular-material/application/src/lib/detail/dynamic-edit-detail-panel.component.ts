import {
  AfterViewInit,
  Component,
  OnDestroy,
  QueryList,
  ViewChildren,
} from '@angular/core';
import {
  PanelService,
  AllorsEditDetailPanelComponent,
  OverviewPageService,
} from '@allors/base/workspace/angular/application';
import {
  AllorsForm,
  angularForms,
  TemplateHostDirective,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { map, Subscription } from 'rxjs';

@Component({
  selector: 'a-mat-dyn-edit-detail-panel',
  templateUrl: './dynamic-edit-detail-panel.component.html',
})
export class AllorsMaterialDynamicEditDetailPanelComponent
  extends AllorsEditDetailPanelComponent
  implements AfterViewInit, OnDestroy
{
  @ViewChildren(TemplateHostDirective)
  templateHosts!: QueryList<TemplateHostDirective>;

  form: AllorsForm;

  private cancelledSubscription: Subscription;
  private savedSubscription: Subscription;

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);

    panelService.register(this);
  }

  ngAfterViewInit() {
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
        angularForms(this.overviewService.objectType).edit
      );

      this.form = componentRef.instance;
      this.form.edit(this.overviewService.id);

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

  ngOnDestroy(): void {
    this.cancelledSubscription?.unsubscribe();
    this.savedSubscription?.unsubscribe();
  }
}
