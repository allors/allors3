import {
  Filter,
  FilterDefinition,
  FilterService,
  SearchFactory,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { FixedAsset, WorkEffortState } from '@allors/default/workspace/domain';
import { M, tags } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppFilterService implements FilterService {
  m: M;

  workeffortFilter: Filter;

  workEffortStateSearch: SearchFactory;
  fixedAssetSearch: SearchFactory;

  constructor(workspaceService: WorkspaceService) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    this.workEffortStateSearch = new SearchFactory({
      objectType: this.m.WorkEffortState,
      roleTypes: [this.m.WorkEffortState.Name],
    });

    this.fixedAssetSearch = new SearchFactory({
      objectType: this.m.FixedAsset,
      roleTypes: [this.m.FixedAsset.SearchString],
    });
  }

  filter(composite: Composite): Filter {
    const { m } = this;

    switch (composite.tag) {
      case tags.WorkEffort:
        return (this.workeffortFilter ??= new Filter(
          new FilterDefinition(
            {
              kind: 'And',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.WorkEffort.WorkEffortState,
                  parameter: 'state',
                },
                {
                  kind: 'Like',
                  roleType: m.WorkEffort.WorkEffortNumber,
                  parameter: 'Number',
                },
                {
                  kind: 'Like',
                  roleType: m.WorkEffort.Name,
                  parameter: 'Name',
                },
                {
                  kind: 'Like',
                  roleType: m.WorkEffort.Description,
                  parameter: 'Description',
                },
                {
                  kind: 'ContainedIn',
                  propertyType:
                    m.WorkEffort.WorkEffortFixedAssetAssignmentsWhereAssignment,
                  extent: {
                    kind: 'Filter',
                    objectType: m.WorkEffortFixedAssetAssignment,
                    predicate: {
                      kind: 'Equals',
                      propertyType: m.WorkEffortFixedAssetAssignment.FixedAsset,
                      parameter: 'equipment',
                    },
                  },
                },
              ],
            },
            {
              state: {
                search: () => this.workEffortStateSearch,
                display: (v: WorkEffortState) => v && v.Name,
              },
              equipment: {
                search: () => this.fixedAssetSearch,
                display: (v: FixedAsset) => v && v.DisplayName,
              },
            }
          )
        ));
    }
    return null;
  }
}
