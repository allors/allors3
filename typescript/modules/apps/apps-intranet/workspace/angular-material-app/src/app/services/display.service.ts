import { Injectable } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { Class, RoleType } from '@allors/system/workspace/meta';
import {
  DisplayService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';

@Injectable()
export class AppDisplayService implements DisplayService {
  roleTypeByClass: Map<Class, RoleType>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;
    this.roleTypeByClass = new Map<Class, RoleType>([
      [m.Person, m.Person.LastName],
      [m.Organisation, m.Organisation.Name],
    ]);
  }

  display(obj: IObject): string {
    const roleType = this.roleTypeByClass.get(obj.strategy.cls);
    return obj.strategy.getUnitRole(roleType) as string;
  }
}
