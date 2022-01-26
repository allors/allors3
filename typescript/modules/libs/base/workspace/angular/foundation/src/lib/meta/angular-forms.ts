import { Composite } from '@allors/system/workspace/meta';

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
  const extension = composite._ as AngularFormsExtension;

  if (forms == null) {
    return extension.forms;
  }

  extension.forms = forms;
}
