import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface PreCreatePullHandler {
  onPreCreatePull(pulls: Pull[]): void;
}

export interface PostCreatePullHandler {
  onPostCreatePull(object: IObject, pullResult: IPullResult): void;
}

export interface CreatePullHandler
  extends PreCreatePullHandler,
    PostCreatePullHandler {}
