import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { RefreshService } from './refresh.service';

@Injectable()
export class RefreshBehaviorService extends RefreshService {
  override refresh$: Observable<Date>;
  private refreshSubject$: BehaviorSubject<Date>;

  constructor() {
    super();
    this.refresh$ = this.refreshSubject$ = new BehaviorSubject(new Date());
  }

  refresh() {
    this.refreshSubject$.next(new Date());
  }
}
