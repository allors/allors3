import { Injectable } from '@angular/core';
import { ObjectType } from '@allors/system/workspace/meta';

@Injectable()
export class OverviewPageService {
  objectType: ObjectType;
  id: number;
}
