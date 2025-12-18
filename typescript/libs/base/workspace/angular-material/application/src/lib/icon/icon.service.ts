import { Composite, RelationType } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export abstract class IconService {
  abstract icon(meta: Composite | RelationType): string;
}
