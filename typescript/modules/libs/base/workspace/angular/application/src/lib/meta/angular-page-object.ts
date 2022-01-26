import { Composite } from '@allors/system/workspace/meta';

interface AngularPageObjectExtension {
  pageObject?: string;
}

export function angularPageObject(composite: Composite): string;
export function angularPageObject(
  composite: Composite,
  pageObject?: string
): void;
export function angularPageObject(
  composite: Composite,
  pageObject?: string
): string | void {
  const extension = composite._ as AngularPageObjectExtension;

  if (pageObject == null) {
    return extension.pageObject;
  }

  extension.pageObject = pageObject;
}
