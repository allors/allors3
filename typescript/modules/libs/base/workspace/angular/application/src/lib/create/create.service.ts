import { Observable } from 'rxjs';
import { Composite } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import { Injectable } from '@angular/core';

export type OnCreate = (object: IObject) => void;

@Injectable()
export abstract class CreateService {
  abstract canCreate(objectType: Composite): boolean;

  abstract create(
    objectType: Composite,
    onCreate: OnCreate
  ): Observable<IObject>;
}
