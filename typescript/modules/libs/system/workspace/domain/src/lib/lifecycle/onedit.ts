import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnObjectPreEdit {
  onPreEdit(objectId: number, pulls: Pull[]);
}

export interface OnObjectPostEdit {
  onPostEdit(object: IObject, pullResult: IPullResult);
}

export interface OnEdit extends OnObjectPreEdit, OnObjectPostEdit {}
