import { Observable } from 'rxjs';
import { Composite } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import { Injectable } from '@angular/core';

@Injectable()
export abstract class EditService {
  abstract canEdit(object: IObject);

  abstract edit(object: IObject, objectType?: Composite): Observable<IObject>;
}
