import { APP_INITIALIZER, ErrorHandler } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  SingletonId,
  UserId,
  UserInfoService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';
import { initialize } from '../app/app.config';
import { ErrorHandlerService } from '../app/services/error-handler.service';
import {
  AllorsMaterialCreateService,
  AllorsMaterialEditDialogService,
} from '@allors/base/workspace/angular-material/application';
import { dialogs } from '../app/app.dialog';

// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export function appInitFactory(
  workspaceService: WorkspaceService,
  httpClient: HttpClient,
  createService: AllorsMaterialCreateService,
  editService: AllorsMaterialEditDialogService,
  userInfoService: UserInfoService,
  userId: UserId,
  internalOrganisationId: InternalOrganisationId,
  singletonId: SingletonId
) {
  return async () => {
    await initialize(
      workspaceService,
      httpClient,
      environment.baseUrl,
      userInfoService,
      userId,
      internalOrganisationId,
      singletonId
    );

    createService.createControlByObjectTypeTag = dialogs.create;
    editService.editControlByObjectTypeTag = dialogs.edit;
  };
}

export const environment = {
  production: true,
  baseUrl: '/allors/',
  providers: [
    {
      // processes all errors
      provide: ErrorHandler,
      useClass: ErrorHandlerService,
    },
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [
        WorkspaceService,
        HttpClient,
        AllorsMaterialCreateService,
        AllorsMaterialEditDialogService,
        UserInfoService,
        UserId,
        InternalOrganisationId,
        SingletonId,
      ],
      multi: true,
    },
  ],
};
