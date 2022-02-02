import { IPullResult, Pull } from '@allors/system/workspace/domain';

export interface OnPrePull {
  onPrePull(pulls: Pull[], prefix?: string): void;
}

export interface OnPostPull {
  onPostPull(pullResult: IPullResult, prefix?: string): void;
}

export interface OnPull extends OnPrePull, OnPostPull {}
