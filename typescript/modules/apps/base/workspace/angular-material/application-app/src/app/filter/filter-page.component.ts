import { Component } from '@angular/core';
import { M } from '@allors/default/workspace/meta';
import {
  Filter,
  FilterDefinition,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

// Test-only page (driven from the base e2e) that hosts the filter component with both a
// Like (non-Between) and a Between field definition, so the filter-field dialog offers both
// kinds. No shipped base filter has a Between field, so this is the only way to exercise the
// dialog's stale-value2 path. See typescript/e2e/Base/Tests/Custom/Form/FilterTest.cs.
@Component({
  templateUrl: './filter-page.component.html',
})
export class FilterPageComponent {
  filter: Filter;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.filter = new Filter(
      new FilterDefinition({
        kind: 'And',
        operands: [
          {
            kind: 'Like',
            roleType: m.Data.String,
            parameter: 'string',
          },
          {
            kind: 'Between',
            roleType: m.Data.Decimal,
            parameter: 'decimal',
          },
        ],
      })
    );
  }
}
