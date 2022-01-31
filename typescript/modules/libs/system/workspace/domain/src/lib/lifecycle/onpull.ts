import { IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnPrePull {
  onPrePull(pulls: Pull[], prefix?: string);
}

export interface OnPostPull {
  onPostPull(pullResult: IPullResult, prefix?: string);
}

export interface OnPull extends OnPrePull, OnPostPull {}
