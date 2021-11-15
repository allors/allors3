import { Composite } from '@allors/workspace/meta/system';

interface AngularListExtension {
  list?: string;
}

export function angularList(composite: Composite): string;
export function angularList(composite: Composite, list: string): void;
export function angularList(composite: Composite, list?: string): string | void {
  if (composite == null) {
    return;
  }

  if (list == null) {
    return (composite._ as AngularListExtension).list;
  }

  (composite._ as AngularListExtension).list = list;
}
