import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface PreCreateOrEditPullHandler {
  onPreCreateOrEditPull(pulls: Pull[]): void;
}

export interface PostCreateOrEditPullHandler {
  onPostCreateOrEditPull(object: IObject, pullResult: IPullResult): void;
}

export interface CreateOrEditPullHandler
  extends PreCreateOrEditPullHandler,
    PostCreateOrEditPullHandler {}
