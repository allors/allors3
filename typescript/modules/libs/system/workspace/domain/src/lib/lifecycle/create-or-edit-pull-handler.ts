import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';
import { Initializer } from './initializer';

export interface PreCreateOrEditPullHandler {
  onPreCreateOrEditPull(pulls: Pull[], initializer?: Initializer): void;
}

export interface PostCreateOrEditPullHandler {
  onPostCreateOrEditPull(
    object: IObject,
    pullResult: IPullResult,
    initializer?: Initializer
  ): void;
}

export interface CreateOrEditPullHandler
  extends PreCreateOrEditPullHandler,
    PostCreateOrEditPullHandler {}
