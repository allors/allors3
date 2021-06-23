import { ISessionServices } from "./ISessionServices";
import { IWorkspace } from "./IWorkspace";

export interface IWorkspaceServices {
  onInit(workspace: IWorkspace): void;

  createSessionServices(): ISessionServices;
}
