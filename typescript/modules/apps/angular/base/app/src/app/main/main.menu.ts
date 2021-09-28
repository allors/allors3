import { tags } from '@allors/workspace/meta/default';

export interface MenuItem {
  tag?: string;
  link?: string;
  title?: string;
  icon?: string;
  children?: MenuItem[];
}

export const menu: MenuItem[] = [
  { title: 'Home', icon: 'home', link: '/' },
  {
    title: 'Contacts',
    icon: 'business',
    children: [{ tag: tags.Person }, { tag: tags.Organisation }],
  },
  {
    title: 'Tests',
    icon: 'build',
    children: [{ title: 'Form', icon: 'share', link: '/tests/form' }],
  },
];
