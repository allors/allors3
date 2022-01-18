import { Observable } from 'rxjs';
import { Composite } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';
import { Injectable } from '@angular/core';

@Injectable()
export abstract class EditService {
  abstract canEdit(object: IObject);

  abstract edit(object: IObject, objectType?: Composite): Observable<IObject>;
}
