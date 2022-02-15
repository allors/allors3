import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { Scoped } from './scoped';

@Injectable()
export class ScopedService {
  scoped$: Observable<Scoped>;
}
