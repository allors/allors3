import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnPreCreateOrEdit {
  onPreCreateOrEdit(pulls: Pull[]);
}

export interface OnPostCreateOrEdit {
  onPostCreateOrEdit(object: IObject, pullResult: IPullResult);
}

export interface OnCreateOrEdit extends OnPreCreateOrEdit, OnPostCreateOrEdit {}
