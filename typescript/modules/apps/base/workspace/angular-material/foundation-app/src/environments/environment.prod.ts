import { APP_INITIALIZER, ErrorHandler } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { init } from '../app/app.init';
import { ErrorHandlerService } from '../allors/errorhandler.service';

// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export function appInitFactory(
  workspaceService: WorkspaceService,
  httpClient: HttpClient
) {
  return async () => {
    init(
      workspaceService,
      httpClient,
      environment.baseUrl,
      environment.authUrl
    );
  };
}

export const environment = {
  production: true,
  baseUrl: 'http://localhost:5000/allors/',
  authUrl: 'TestAuthentication/Token',
  // authUrl: 'Authentication/Token',
  providers: [
    {
      // processes all errors
      provide: ErrorHandler,
      useClass: ErrorHandlerService,
    },
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [WorkspaceService, HttpClient],
      multi: true,
    },
  ],
};
