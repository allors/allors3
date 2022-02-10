import { IPullResult, Pull } from '@allors/system/workspace/domain';

export interface PullHandler {
  onPrePull(pulls: Pull[]): void;

  onPostPull(pullResult: IPullResult): void;
}
