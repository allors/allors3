import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnPreCreate {
  onPreCreate(pulls: Pull[]): void;
}

export interface OnPostCreate {
  onPostCreate(object: IObject, pullResult: IPullResult): void;
}

export interface OnCreate extends OnPreCreate, OnPostCreate {}
