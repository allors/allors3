import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { angularForms } from '@allors/base/workspace/angular/foundation';
import { CountryFormComponent } from './domain/country/forms/country-form.component';

function forms(composite: Composite, both: unknown);
function forms(composite: Composite, create: unknown, edit: unknown);
function forms(composite: Composite, bothOrCreate: unknown, edit?: unknown) {
  if (edit == null) {
    angularForms(composite, { create: bothOrCreate, edit: bothOrCreate });
  } else {
    angularForms(composite, { create: bothOrCreate, edit: edit });
  }
}

export function initForms(m: M) {
  forms(m.Country, CountryFormComponent);
}
