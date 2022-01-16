import { Observable, ReplaySubject, Subject } from 'rxjs';
import { Class } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';

export class FormService {
  create$: Observable<Class>;
  edit$: Observable<number>;
  save$: Observable<void>;
  saved$: Observable<IObject>;
  cancel$: Observable<void>;
  cancelled$: Observable<IObject>;

  canSave: () => () => boolean;

  private createSubject: ReplaySubject<Class>;
  private editSubject: ReplaySubject<number>;
  private saveSubject: Subject<void>;
  private savedSubject: Subject<IObject>;
  private cancelSubject: Subject<void>;
  private cancelledSubject: Subject<IObject>;

  constructor() {
    this.createSubject = new ReplaySubject<Class>();
    this.editSubject = new ReplaySubject<number>();
    this.saveSubject = new Subject<void>();
    this.savedSubject = new Subject<IObject>();
    this.cancelSubject = new Subject<void>();
    this.cancelledSubject = new Subject<IObject>();

    this.create$ = this.createSubject.asObservable();
    this.edit$ = this.editSubject.asObservable();
    this.save$ = this.saveSubject.asObservable();
    this.saved$ = this.savedSubject.asObservable();
    this.cancel$ = this.cancelSubject.asObservable();
    this.cancelled$ = this.cancelledSubject.asObservable();
  }

  create(cls: Class): void {
    this.createSubject.next(cls);
  }

  edit(objectId: number): void {
    this.editSubject.next(objectId);
  }

  save(): void {
    this.saveSubject.next();
  }

  saved(object: IObject): void {
    this.savedSubject.next(object);
  }

  cancel(): void {
    this.cancelSubject.next();
  }

  cancelled(object: IObject): void {
    this.cancelledSubject.next(object);
  }
}
