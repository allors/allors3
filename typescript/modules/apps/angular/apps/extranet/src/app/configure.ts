import { angularList, angularOverview, angularMenu } from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';
import { Composite } from '@allors/workspace/meta/system';

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
}
