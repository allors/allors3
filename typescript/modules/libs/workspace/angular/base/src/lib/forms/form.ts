import { Class } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';
import { EventEmitter } from '@angular/core';

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
