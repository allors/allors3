import { Directive, HostBinding } from '@angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { RoleType } from '@allors/workspace/meta/system';
import { AllorsPanelComponent } from './panel.component';

@Directive()
export abstract class AllorsPanelRoleComponent<
  T extends IObject
> extends AllorsPanelComponent<T> {
  dataAllorsKind = 'panel-detail';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.id;
  }

  @HostBinding('attr.data-allors-roletype')
  get dataAllorsRoleType() {
    return this.roleType?.relationType.tag;
  }

  roleType: RoleType;
}
