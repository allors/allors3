import { Class } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import { EventEmitter } from '@angular/core';

export interface AllorsFormConstructor {
  new (): AllorsForm;
}

export interface AllorsForm {
  object: IObject;

  readonly canWrite: boolean;

  readonly canSave: boolean;

  saved: EventEmitter<IObject>;

  cancelled: EventEmitter<void>;

  create(objectType: Class): void;

  edit(objectId: number): void;

  save(): void;

  cancel(): void;
}
