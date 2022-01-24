import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import {
  angularList,
  angularOverview,
} from '@allors/base/workspace/angular/foundation';

function nav(composite: Composite, list: string, overview?: string) {
  angularList(composite, list);
  angularOverview(composite, overview);
}

export function initNav(m: M) {
  // Navigation
  nav(m.Person, '/contacts/people', '/contacts/person/:id');
  nav(m.Organisation, '/contacts/organisations', '/contacts/organisation/:id');
  nav(m.Country, '/contacts/countries');
}
