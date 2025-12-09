import { Injectable } from '@angular/core';
import { Composite } from '@allors/system/workspace/meta';
import { HyperlinkType } from './hyperlink-type';

@Injectable()
export abstract class HyperlinkService {
  abstract linkTypes(objectType: Composite): HyperlinkType[];
}
