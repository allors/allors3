import { IWorkspace } from '../iworkspace';
import { IDerivationService } from './derivation/iderivation-service';

import { ISessionServices } from './isession-services';

export interface IWorkspaceServices {
  onInit(workspace: IWorkspace): void;

  createSessionServices(): ISessionServices;

  derivationService: IDerivationService;
}
