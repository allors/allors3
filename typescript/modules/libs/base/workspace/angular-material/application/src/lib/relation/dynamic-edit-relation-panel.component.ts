import {
  Component,
  HostBinding,
  Input,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { Composite, PropertyType } from '@allors/system/workspace/meta';
import {
  IPullResult,
  Pull,
  Initializer,
} from '@allors/system/workspace/domain';
import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  AllorsEditRelationPanelComponent,
  PanelService,
  ObjectService,
} from '@allors/base/workspace/angular/application';
import { DeleteService } from '../actions/delete/delete.service';
import { EditRoleService } from '../actions/edit-role/edit-role.service';
import { IconService } from '../icon/icon.service';
import { delay, pipe, Subscription, tap } from 'rxjs';

@Component({
  selector: 'a-mat-dyn-edit-relation-panel',
  templateUrl: './dynamic-edit-relation-panel.component.html',
})
export class AllorsMaterialDynamicEditRelationPanelComponent
  extends AllorsEditRelationPanelComponent
  implements OnInit, OnDestroy
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  @Input()
  propertyType: PropertyType;

  propertyId: number;

  objectType: Composite;

  get panelId() {
    return `${this.propertyType.name}`;
  }

  get icon() {
    return this.iconService.icon(this.propertyType.relationType);
  }

  get titel() {
    return this.propertyType.pluralName;
  }

  get initializer(): Initializer {
    return { propertyType: this.propertyType, id: this.objectInfo.id };
  }

  private subscription: Subscription;

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editRoleService: EditRoleService,
    private iconService: IconService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );

    panelService.register(this);
    sharedPullService.register(this);

    this.subscription = this.objectService.objectInfo$
      .pipe(
        pipe(delay(1)),
        tap((object) => {
          this.propertyId = object.id;
        }),
        tap(() => {
          this.refreshService.refresh();
        })
      )
      .subscribe();
  }

  ngOnInit() {
    this.objectType = this.propertyType.objectType as Composite;
  }

  onPreScopedPull(pulls: Pull[], scope?: string): void {
    // TODO: Remove
  }
  onPostScopedPull(pullResult: IPullResult, scope?: string): void {
    // TODO: Remove
  }

  toggle() {
    this.panelService.stopEdit().pipe().subscribe();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    this.subscription?.unsubscribe();
  }
}
