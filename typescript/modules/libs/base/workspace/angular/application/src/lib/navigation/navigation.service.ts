import { Injectable } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { ObjectType } from '@allors/system/workspace/meta';

@Injectable()
export abstract class NavigationService {
  abstract hasList(obj: IObject): boolean;

  abstract list(objectType: ObjectType);

  abstract hasOverview(obj: IObject): boolean;

  abstract overview(obj: IObject): void;
}
