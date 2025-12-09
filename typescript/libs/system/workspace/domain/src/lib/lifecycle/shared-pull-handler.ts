import { IPullResult, Pull } from '@allors/system/workspace/domain';

export interface SharedPullHandler {
  onPreSharedPull(pulls: Pull[], prefix: string): void;

  onPostSharedPull(pullResult: IPullResult, prefix: string): void;
}
