import { Injectable } from '@angular/core';
import { Composite } from '@allors/system/workspace/meta';
import { LinkType } from './link-type';

@Injectable()
export abstract class LinkService {
  abstract linkTypes(objectType: Composite): LinkType[];
}
