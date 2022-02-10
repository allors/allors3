import { IPullResult, Pull } from '@allors/system/workspace/domain';

export interface ScopedPullHandler {
  onPreScopedPull(pulls: Pull[], scope: string): void;

  onPostScopedPull(pullResult: IPullResult, scope: string): void;
}
