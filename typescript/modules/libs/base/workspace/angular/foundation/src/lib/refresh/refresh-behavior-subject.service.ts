import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, debounceTime } from 'rxjs';
import { RefreshService } from './refresh.service';

@Injectable()
export class RefreshBehaviorService extends RefreshService {
  override refresh$: Observable<Date>;
  private refreshSubject$: BehaviorSubject<Date>;

  constructor() {
    super();
    this.refreshSubject$ = new BehaviorSubject(new Date());
    this.refresh$ = this.refreshSubject$.pipe(debounceTime(100));
  }

  refresh() {
    this.refreshSubject$.next(new Date());
  }
}
