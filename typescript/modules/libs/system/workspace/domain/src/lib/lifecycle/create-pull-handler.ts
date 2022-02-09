import {
  Initializer,
  IObject,
  IPullResult,
  Pull,
} from '@allors/system/workspace/domain';

export interface PreCreatePullHandler {
  onPreCreatePull(pulls: Pull[], initializer?: Initializer): void;
}

export interface PostCreatePullHandler {
  onPostCreatePull(
    object: IObject,
    pullResult: IPullResult,
    initializer?: Initializer
  ): void;
}

export interface CreatePullHandler
  extends PreCreatePullHandler,
    PostCreatePullHandler {}
