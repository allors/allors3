import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnPreEdit {
  onPreEdit(objectId: number, pulls: Pull[]);
}

export interface OnPostEdit {
  onPostEdit(object: IObject, pullResult: IPullResult);
}

export interface OnEdit extends OnPreEdit, OnPostEdit {}
