import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import {
  angularPageList,
  angularPageEdit,
} from '@allors/base/workspace/angular/application';

function nav(composite: Composite, list: string, overview?: string) {
  angularPageList(composite, list);
  angularPageEdit(composite, overview);
}

export function initNav(m: M) {
  // Navigation
  nav(m.Person, '/contacts/people', '/contacts/person/:id');
  nav(m.Organisation, '/contacts/organisations', '/contacts/organisation/:id');
  nav(m.Country, '/contacts/countries');
}
