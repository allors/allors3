import { IObject } from '@allors/system/workspace/domain';
import { Injectable } from '@angular/core';

@Injectable()
export abstract class DisplayService {
  abstract display(obj: IObject): string;
}
