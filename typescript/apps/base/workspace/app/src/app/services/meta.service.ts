import {
  MetaService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import {
  AssociationType,
  Composite,
  pluralize,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppMetaService implements MetaService {
  singularNameByObject: Map<Composite | PropertyType, string>;
  pluralNameByObject: Map<Composite | PropertyType, string>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.singularNameByObject = new Map<Composite | PropertyType, string>([
      [m.Organisation, 'Company'],
      [m.Person.OrganisationsWhereShareholder, 'ShareholderCompany'],
    ]);

    this.pluralNameByObject = new Map<Composite | PropertyType, string>([
      [m.Organisation, 'Companies'],
    ]);
  }

  singularName(metaObject: Composite | PropertyType): string {
    return this.singularNameByObject.get(metaObject) ?? metaObject.singularName;
  }

  pluralName(metaObject: Composite | PropertyType): string {
    return (
      this.pluralNameByObject.get(metaObject) ??
      pluralize(this.singularNameByObject.get(metaObject)) ??
      metaObject.pluralName
    );
  }
}
