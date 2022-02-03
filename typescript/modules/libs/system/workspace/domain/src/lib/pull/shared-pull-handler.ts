import { IPullResult, Pull } from '@allors/system/workspace/domain';

export interface PreSharedPullHandler {
  onPreSharedPull(pulls: Pull[], prefix?: string): void;
}

export interface PostSharedPullHandler {
  onPostSharedPull(pullResult: IPullResult, prefix?: string): void;
}

export interface SharedPullHandler
  extends PreSharedPullHandler,
    PostSharedPullHandler {}
