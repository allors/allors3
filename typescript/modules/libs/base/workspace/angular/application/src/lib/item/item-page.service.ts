import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { Composite } from '@allors/system/workspace/meta';

export interface ItemPageInfo {
  objectType: Composite;
  id: number;
}

@Injectable()
export class ItemPageService {
  info$: Observable<ItemPageInfo>;
}
