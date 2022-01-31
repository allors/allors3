import { IObject, OnCreate } from '@allors/system/workspace/domain';
import { ObjectType } from '@allors/system/workspace/meta';
import { EventEmitter } from '@angular/core';

export interface AllorsFormConstructor {
  new (): AllorsForm;
}

export interface AllorsForm {
  readonly canWrite: boolean;

  readonly canSave: boolean;

  saved: EventEmitter<IObject>;

  cancelled: EventEmitter<void>;

  object: IObject;

  create(objectType: ObjectType, handlers?: OnCreate[]): void;

  edit(objectId: number): void;

  save(): void;

  cancel(): void;
}
