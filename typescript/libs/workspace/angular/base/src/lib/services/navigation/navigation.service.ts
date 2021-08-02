import { Injectable } from '@angular/core';

import { IObject } from '@allors/workspace/domain/system';
import { ObjectType } from '@allors/workspace/meta/system';

@Injectable()
export abstract class NavigationService {
  abstract hasList(obj: IObject): boolean;

  abstract list(objectType: ObjectType);

  abstract hasOverview(obj: IObject): boolean;

  abstract overview(obj: IObject): void;
}
