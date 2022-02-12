import { Injectable } from '@angular/core';
import { Composite } from '@allors/system/workspace/meta';
import { Node } from '@allors/system/workspace/domain';

@Injectable()
export abstract class LinkService {
  abstract link(objectType: Composite): Node[];
}
