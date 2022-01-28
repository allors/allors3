import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnObjectPreCreate {
  onObjectPreCreate(pulls: Pull[]);
}

export interface OnObjectPostCreate {
  onObjectPostCreate(object: IObject, pullResult: IPullResult);
}

export interface OnObjectCreate extends OnObjectPreCreate, OnObjectPostCreate {}
