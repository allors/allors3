import { Subject } from 'rxjs';
import { Injectable } from '@angular/core';
import { AllorsMaterialSideNavService } from './side-nav.service';

@Injectable()
export class AllorsMaterialSideNavSubjectService extends AllorsMaterialSideNavService {
  private toggleSource = new Subject<void>();
  public toggle$ = this.toggleSource.asObservable();

  private openSource = new Subject<void>();
  public open$ = this.openSource.asObservable();

  private closeSource = new Subject<void>();
  public close$ = this.closeSource.asObservable();

  public toggle() {
    this.toggleSource.next();
  }

  public open() {
    this.openSource.next();
  }

  public close() {
    this.closeSource.next();
  }
}
