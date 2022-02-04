import { M } from '@allors/default/workspace/meta';
import { angularMenu } from '@allors/base/workspace/angular/application';

export function configMenu(m: M) {
  // Menu
  angularMenu(m, [
    { title: 'Home', icon: 'home', link: '/' },
    {
      title: 'Contacts',
      icon: 'business',
      children: [
        { objectType: m.Person },
        { objectType: m.Organisation },
        { objectType: m.Country },
      ],
    },
    { title: 'Fields', icon: 'build', link: '/fields' },
  ]);
}
