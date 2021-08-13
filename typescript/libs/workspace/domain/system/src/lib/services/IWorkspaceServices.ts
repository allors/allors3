import { ISessionServices } from "./ISessionServices";
import { IWorkspace } from "../IWorkspace";
import { IDerivationService } from "./derivation/IDerivationService";

export interface IWorkspaceServices {
  onInit(workspace: IWorkspace): void;

  createSessionServices(): ISessionServices;

  derivationService: IDerivationService;
}
