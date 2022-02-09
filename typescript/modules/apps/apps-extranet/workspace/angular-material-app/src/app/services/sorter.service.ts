import {
  Sorter,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M, tags } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppSorterService implements SorterService {
  m: M;

  constructor(workspaceService: WorkspaceService) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  sorter(composite: Composite): Sorter {
    const { m } = this;

    switch (composite.tag) {
      case tags.WorkEffort:
        return new Sorter({
          number: [m.WorkEffort.SortableWorkEffortNumber],
          name: [m.WorkEffort.Name],
          description: [m.WorkEffort.Description],
          lastModifiedDate: m.Person.LastModifiedDate,
        });
    }
    return null;
  }
}
