import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';
import { Sorter } from './sorter';

@Injectable()
export abstract class SorterService {
  abstract sorter(composite: Composite): Sorter;
}
