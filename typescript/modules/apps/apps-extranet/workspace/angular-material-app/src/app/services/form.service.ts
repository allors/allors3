import { M, tags } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import {
  FormService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

import { Injectable } from '@angular/core';
import {
  WorkTaskCreateFormComponent,
  WorkTaskEditFormComponent,
} from '@allors/apps-extranet/workspace/angular-material';

@Injectable()
export class AppFormService implements FormService {
  m: M;

  constructor(workspaceService: WorkspaceService) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  createForm(composite: Composite) {
    switch (composite.tag) {
      // Objects
      case tags.WorkTask:
        return WorkTaskCreateFormComponent;
    }

    return this.editForm(composite);
  }

  editForm(composite: Composite) {
    switch (composite.tag) {
      // Objects
      case tags.WorkTask:
        return WorkTaskEditFormComponent;
    }

    return null;
  }
}

export const components: any[] = [
  WorkTaskCreateFormComponent,
  WorkTaskEditFormComponent,
];
