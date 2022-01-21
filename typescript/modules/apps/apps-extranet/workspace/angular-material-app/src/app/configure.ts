import {
  angularList,
  angularOverview,
  angularMenu,
  angularFilterDefinition,
  FilterDefinition,
  angularSorter,
  Sorter,
  SearchFactory,
} from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';
import { Composite } from '@allors/system/workspace/meta';
import { FixedAsset, WorkEffortState } from '@allors/workspace/domain/default';

function nav(composite: Composite, list: string, overview?: string) {
  angularList(composite, list);
  angularOverview(composite, overview);
}

export function configure(m: M) {
  // Menu
  angularMenu(m, [
    { title: 'Home', icon: 'home', link: '/' },
    {
      title: 'WorkEfforts',
      icon: 'schedule',
      children: [{ objectType: m.WorkEffort }],
    },
  ]);

  // Navigation
  nav(m.WorkEffort, '/workefforts/workefforts');
  nav(m.WorkTask, '/workefforts/workefforts', '/workefforts/worktask/:id');

  const workEffortStateSearch = new SearchFactory({
    objectType: m.WorkEffortState,
    roleTypes: [m.WorkEffortState.Name],
  });

  const fixedAssetSearch = new SearchFactory({
    objectType: m.FixedAsset,
    roleTypes: [m.FixedAsset.SearchString],
  });

  angularFilterDefinition(
    m.WorkEffort,
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
          { kind: 'Like', roleType: m.WorkEffort.Name, parameter: 'Name' },
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
          search: () => workEffortStateSearch,
          display: (v: WorkEffortState) => v && v.Name,
        },
        equipment: {
          search: () => fixedAssetSearch,
          display: (v: FixedAsset) => v && v.DisplayName,
        },
      }
    )
  );

  angularSorter(
    m.WorkEffort,
    new Sorter({
      number: [m.WorkEffort.SortableWorkEffortNumber],
      name: [m.WorkEffort.Name],
      description: [m.WorkEffort.Description],
      lastModifiedDate: m.Person.LastModifiedDate,
    })
  );
}
