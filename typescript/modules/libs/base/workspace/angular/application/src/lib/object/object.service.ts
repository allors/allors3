import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { ObjectInfo } from './object-info';

@Injectable()
export class ObjectService {
  objectInfo$: Observable<ObjectInfo>;
}
