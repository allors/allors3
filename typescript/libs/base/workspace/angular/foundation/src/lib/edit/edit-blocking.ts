import { Observable } from 'rxjs';

export interface EditBlocking {
  stopEdit(): Observable<boolean>;
}
