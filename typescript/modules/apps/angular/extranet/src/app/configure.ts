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
      title: 'Contacts',
      icon: 'group',
      children: [{ objectType: m.Person }],
    },
  ]);

  // Navigation
  nav(m.Person, '/contacts/people', '/contacts/person/:id');
}
