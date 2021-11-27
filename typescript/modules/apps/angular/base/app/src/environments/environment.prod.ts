import { APP_INITIALIZER } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { AllorsMaterialSideNavService } from '@allors/workspace/angular/base';
import { AngularSelectorsModule } from '@allors/angular/selectors';
import { init } from '../app/app.init';

// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export function appInitFactory(workspaceService: WorkspaceService, httpClient: HttpClient) {
  return async () => {
    init(workspaceService, httpClient, environment.baseUrl, environment.authUrl);
  };
}

export const environment = {
  production: true,
  baseUrl: 'http://localhost:5000/allors/',
  authUrl: 'TestAuthentication/Token',
  // authUrl: 'Authentication/Token',
  providers: [
    AngularSelectorsModule,
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [WorkspaceService, HttpClient, AngularSelectorsModule, Router, AllorsMaterialSideNavService],
      multi: true,
    },
  ],
};
