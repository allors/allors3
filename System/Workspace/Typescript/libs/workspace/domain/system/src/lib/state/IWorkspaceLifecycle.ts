import { ISessionLifecycle } from "./ISessionLifecycle";
import { IWorkspace } from "../IWorkspace";

export interface IWorkspaceLifecycle {
  onInit(internalWorkspace: IWorkspace): void;

  createSessionContext(): ISessionLifecycle;
}
