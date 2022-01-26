import { Composite } from '@allors/system/workspace/meta';

interface AngularPageListExtension {
  list?: string;
}

export function angularPageList(composite: Composite): string;
export function angularPageList(composite: Composite, list: string): void;
export function angularPageList(
  composite: Composite,
  list?: string
): string | void {
  const extension = composite?._ as AngularPageListExtension;

  if (list == null) {
    return extension?.list;
  }

  extension.list = list;
}
