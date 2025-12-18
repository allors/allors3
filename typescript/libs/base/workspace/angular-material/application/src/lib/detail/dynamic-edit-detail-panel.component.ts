import { Component, OnDestroy, QueryList, ViewChildren } from '@angular/core';
import {
  PanelService,
  ScopedService,
  AllorsEditDetailPanelComponent,
} from '@allors/base/workspace/angular/application';
import {
  AllorsForm,
  DisplayService,
  FormService,
  RefreshService,
  SharedPullService,
  TemplateHostDirective,
} from '@allors/base/workspace/angular/foundation';
import { map, Subscription, tap } from 'rxjs';
import { IPullResult, Pull } from '@allors/system/workspace/domain';

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
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    private displayService: DisplayService,
    private formService: FormService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
  }

  onPreSharedPull(pulls: Pull[], prefix?: string): void {
    pulls.push({ objectId: this.scoped.id, results: [{ name: prefix }] });

    this.subscribeTemplate();
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string): void {}

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

      // Force a changed detection run
      setTimeout(() => {
        const viewContainerRef = templateHost.viewContainerRef;
        viewContainerRef.clear();

        const componentRef = viewContainerRef.createComponent<AllorsForm>(
          this.formService.editForm(this.scoped.objectType)
        );

        this.form = componentRef.instance;
        this.form.edit({
          kind: 'EditRequest',
          objectId: this.scoped.id,
        });

        this.cancelledSubscription = this.form.cancelled
          .pipe(
            map(() => {
              this.panelService
                .stopEdit()
                .pipe(tap(() => this.refreshService.refresh()))
                .subscribe();
            })
          )
          .subscribe();

        this.savedSubscription = this.form.saved
          .pipe(
            map(() => {
              this.panelService
                .stopEdit()
                .pipe(tap(() => this.refreshService.refresh()))
                .subscribe();
            })
          )
          .subscribe();
      }, 0);
    });
  }
}
