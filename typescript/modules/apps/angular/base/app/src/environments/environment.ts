import { APP_INITIALIZER } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { AllorsMaterialSideNavService } from '@allors/workspace/angular/base';
import { init } from '../app/app.init';

// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export function appInitFactory(workspaceService: WorkspaceService, httpClient: HttpClient, router: Router, sideNavService: AllorsMaterialSideNavService) {
  return async () => {
    init(workspaceService, httpClient, environment.baseUrl, environment.authUrl);

    const allors: any = (window['allors'] = {});

    allors.info = {
      router,
      workspaceService,
      sideNavService,
    };
  };
}

export const environment = {
  production: false,
  baseUrl: 'http://localhost:5000/allors/',
  authUrl: 'TestAuthentication/Token',
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [WorkspaceService, HttpClient, Router, AllorsMaterialSideNavService],
      multi: true,
    },
  ],
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
import 'zone.js/plugins/zone-error'; // Included with Angular CLI.import { init } from '../app/app.init';
