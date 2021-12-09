import { Inject, Injectable } from '@angular/core';
import { OBJECT_CREATE_TOKEN, OBJECT_EDIT_TOKEN } from '../material/services/object/object.tokens';

export interface DialogInfo {
  create?: DialogObjectInfo[];
  edit?: DialogObjectInfo[];
}

export interface DialogObjectInfo {
  tag?: string;
  component?: string;
}

@Injectable()
export class DialogInfoService {
  constructor(@Inject(OBJECT_CREATE_TOKEN) private create: { [id: string]: any }, @Inject(OBJECT_EDIT_TOKEN) private edit: { [id: string]: any }) {}

  write(allors: { [key: string]: unknown }) {
    allors.dialog = this.dialog;
  }

  private get dialog(): string {
    const dialog: DialogInfo = {
      create: Object.keys(this.create).map((v) => ({ tag: v, component: this.create[v].name })),
      edit: Object.keys(this.edit).map((v) => ({ tag: v, component: this.edit[v].name })),
    };

    return JSON.stringify(dialog);
  }
}
