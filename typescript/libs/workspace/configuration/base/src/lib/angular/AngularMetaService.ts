import { IAngularComposite, IAngularMetaObject, IAngularMetaService, IAngularRoleType } from '@allors/workspace/angular/base';
import { Composite, RoleType } from '@allors/workspace/meta/system';

export class AngularMetaService implements IAngularMetaService {
  angularMetaByMeta: Map<number, IAngularMetaObject>;

  constructor() {
    this.angularMetaByMeta = new Map();
  }

  for(composite: Composite): IAngularComposite;
  for(roleType: RoleType): IAngularRoleType;
  for(compositeOrRoleType: unknown): unknown {
    if (compositeOrRoleType == null) {
      return null;
    }

    if ((compositeOrRoleType as Composite).tag) {
      const tag = (compositeOrRoleType as Composite).tag;
      let angularComposite = this.angularMetaByMeta.get(tag);
      if (angularComposite == null) {
        angularComposite = { kind: 'AngularComposite' };
        this.angularMetaByMeta.set(tag, angularComposite);
      }

      return angularComposite;
    } else {
      const tag = (compositeOrRoleType as RoleType).relationType.tag;
      let angularComposite = this.angularMetaByMeta.get(tag);
      if (angularComposite == null) {
        angularComposite = { kind: 'AngularRoleType' };
        this.angularMetaByMeta.set(tag, angularComposite);
      }

      return angularComposite;
    }
  }
}
