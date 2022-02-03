import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnPreCreateOrEdit {
  onPreCreateOrEdit(pulls: Pull[]): void;
}

export interface OnPostCreateOrEdit {
  onPostCreateOrEdit(object: IObject, pullResult: IPullResult): void;
}

export interface OnCreateOrEdit extends OnPreCreateOrEdit, OnPostCreateOrEdit {}
