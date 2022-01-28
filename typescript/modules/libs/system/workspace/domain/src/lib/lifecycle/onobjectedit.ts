import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnObjectPreEdit {
  onObjectPreEdit(objectId: number, pulls: Pull[]);
}

export interface OnObjectPostEdit {
  onObjectPostEdit(object: IObject, pullResult: IPullResult);
}

export interface OnObjectEdit extends OnObjectPreEdit, OnObjectPostEdit {}
