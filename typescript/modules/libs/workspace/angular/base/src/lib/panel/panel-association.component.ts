import { Directive, HostBinding } from '@angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { AssociationType } from '@allors/workspace/meta/system';
import { AllorsPanelComponent } from './panel.component';

@Directive()
export abstract class AllorsPanelAssociationComponent<
  T extends IObject
> extends AllorsPanelComponent<T> {
  dataAllorsKind = 'panel-detail';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.id;
  }

  @HostBinding('attr.data-allors-associationtype')
  get dataAllorsAssociationType() {
    return this.associationType?.relationType.tag;
  }

  associationType: AssociationType;
}
