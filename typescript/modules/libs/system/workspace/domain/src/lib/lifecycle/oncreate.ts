import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnPreCreate {
  onPreCreate(pulls: Pull[]);
}

export interface OnPostCreate {
  onPostCreate(object: IObject, pullResult: IPullResult);
}

export interface OnCreate extends OnPreCreate, OnPostCreate {}
