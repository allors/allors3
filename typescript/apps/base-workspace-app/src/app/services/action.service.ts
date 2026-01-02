import { Injectable } from '@angular/core';
import {
  Action,
  ActionService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { MethodActionService } from '@allors/base/workspace/angular-material/application';

@Injectable()
export class AppActionService implements ActionService {
  actionByObjectType: Map<Composite, Action[]>;

  constructor(
    workspaceService: WorkspaceService,
    methodActionService: MethodActionService
  ) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.actionByObjectType = new Map<Composite, Action[]>([
      [
        m.Organisation,
        [
          methodActionService.create(m.Organisation.ToggleCanWrite),
          methodActionService.create(m.Organisation.JustDoIt),
        ],
      ],
      [m.Person, []],
    ]);
  }

  action(objectType: Composite): Action[] {
    return this.actionByObjectType.get(objectType) ?? [];
  }
}
