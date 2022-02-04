import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export abstract class FormService {
  abstract createForm(composite: Composite): any;
  abstract editForm(composite: Composite): any;
}
