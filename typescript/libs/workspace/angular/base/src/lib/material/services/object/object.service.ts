import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { IObject, ISession } from '@allors/workspace/domain/system';
import { ObjectType } from '@allors/workspace/meta/system';

import { ObjectData } from './object.data';

@Injectable()
export abstract class ObjectService {
  abstract create(objectType: ObjectType, createData?: ObjectData): Observable<IObject>;

  abstract hasCreateControl(objectType: ObjectType, createData: ObjectData, session: ISession);

  abstract edit(object: IObject, createData?: ObjectData): Observable<IObject>;

  abstract hasEditControl(object: IObject);
}
