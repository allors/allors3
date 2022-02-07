import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export abstract class MetaService {
  abstract singularName(composite: Composite): string;

  abstract pluralName(composite: Composite): string;
}
