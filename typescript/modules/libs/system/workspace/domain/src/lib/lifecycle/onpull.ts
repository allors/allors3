import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnPrePull {
  onObjectPreCreate(pulls: Pull[]);
}

export interface OnPostPull {
  onObjectPostCreate(object: IObject, pullResult: IPullResult);
}

export interface OnPull extends OnPrePull, OnPostPull {}
