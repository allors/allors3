import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
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
import { map, Subscription, tap } from 'rxjs';

@Component({
  selector: 'a-mat-dyn-edit-detail-panel',
  templateUrl: './dynamic-edit-detail-panel.component.html',
})
export class AllorsMaterialDynamicEditDetailPanelComponent
  extends AllorsEditDetailPanelComponent
  implements OnInit, OnDestroy
{
  @ViewChild(TemplateHostDirective, { static: true })
  templateHost!: TemplateHostDirective;

  form: AllorsForm;

  private cancelledSubscription: Subscription;
  private savedSubscription: Subscription;

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }

  ngOnInit(): void {
    const viewContainerRef = this.templateHost.viewContainerRef;
    viewContainerRef.clear();

    const componentRef = viewContainerRef.createComponent<AllorsForm>(
      angularForms(this.overviewService.objectType).edit
    );

    this.form = componentRef.instance;
    this.form.edit(this.overviewService.id);

    this.cancelledSubscription = this.form.cancelled
      .pipe(
        map(() => {
          this.panelService.stopEdit();
        })
      )
      .subscribe();

    this.savedSubscription = this.form.saved
      .pipe(
        map(() => {
          this.panelService.stopEdit();
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.cancelledSubscription?.unsubscribe();
    this.savedSubscription?.unsubscribe();
  }
}
