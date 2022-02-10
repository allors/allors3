import { IObject, PullHandler } from '@allors/system/workspace/domain';
import { EventEmitter } from '@angular/core';
import { CreateRequest } from '../create/create-request';
import { EditRequest } from '../edit/edit-request';

export interface AllorsFormConstructor {
  new (): AllorsForm;
}

export interface AllorsForm extends PullHandler {
  readonly canWrite: boolean;

  readonly canSave: boolean;

  saved: EventEmitter<IObject>;

  cancelled: EventEmitter<void>;

  create(request: CreateRequest): void;

  edit(request: EditRequest): void;

  save(): void;

  cancel(): void;
}
