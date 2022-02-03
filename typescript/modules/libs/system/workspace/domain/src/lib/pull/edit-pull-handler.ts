import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

export interface PreEditPullHandler {
  onPreEditPull(objectId: number, pulls: Pull[]): void;
}

export interface PostEditPullHandler {
  onPostEditPull(object: IObject, pullResult: IPullResult): void;
}

export interface EditPullHandler
  extends PreEditPullHandler,
    PostEditPullHandler {}
