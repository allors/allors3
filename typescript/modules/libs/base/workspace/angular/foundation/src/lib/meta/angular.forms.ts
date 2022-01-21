import { Composite } from '@allors/workspace/meta/system';

export interface AngularForms {
  edit: any;
  create: any;
}

interface AngularFormsExtension {
  forms?: AngularForms;
}

export function angularForms(composite: Composite): AngularForms;
export function angularForms(composite: Composite, forms: AngularForms): void;
export function angularForms(
  composite: Composite,
  forms?: AngularForms
): AngularForms | void {
  if (composite == null) {
    return;
  }

  if (forms == null) {
    return (composite._ as AngularFormsExtension).forms;
  }

  (composite._ as AngularFormsExtension).forms = forms;
}
