import { Composite } from '@allors/system/workspace/meta';

interface AngularPageObjectExtension {
  pageObject?: string;
}

export function angularPageEdit(composite: Composite): string;
export function angularPageEdit(
  composite: Composite,
  pageObject?: string
): void;
export function angularPageEdit(
  composite: Composite,
  pageObject?: string
): string | void {
  const extension = composite?._ as AngularPageObjectExtension;

  if (pageObject == null) {
    return extension?.pageObject;
  }

  extension.pageObject = pageObject;
}
