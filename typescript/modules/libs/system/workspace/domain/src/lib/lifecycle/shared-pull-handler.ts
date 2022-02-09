import { IPullResult, Pull } from '@allors/system/workspace/domain';

export interface PreSharedPullHandler {
  onPreSharedPull(pulls: Pull[], scope?: string): void;
}

export interface PostSharedPullHandler {
  onPostSharedPull(pullResult: IPullResult, scope?: string): void;
}

export interface SharedPullHandler
  extends PreSharedPullHandler,
    PostSharedPullHandler {}
