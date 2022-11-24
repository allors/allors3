import { BehaviorSubject, Observable } from 'rxjs';

export abstract class SessionState {
  public observable$: Observable<number | null>;

  private subject: BehaviorSubject<number | null>;

  constructor(private key: string) {
    const initialStringValue = sessionStorage.getItem(this.key);
    const initialValue =
      initialStringValue != null ? parseInt(initialStringValue) : null;
    this.subject = new BehaviorSubject<number | null>(initialValue);
    this.observable$ = this.subject.asObservable();
  }

  get value(): number | null {
    return this.subject.getValue();
  }

  set value(value: number | null) {
    if (value == null) {
      sessionStorage.removeItem(this.key);
    } else {
      if (!Number.isInteger(value)) {
        value = parseInt(value as any);
      }

      sessionStorage.setItem(this.key, value.toString());
    }

    this.subject.next(value);
  }
}
