import { APP_BOOTSTRAP_LISTENER, APP_INITIALIZER, ComponentRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { DialogInfoService, MenuInfoService, NavigationInfoService, RouteInfoService } from '@allors/workspace/angular/base';
import { init } from '../app/app.init';

// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export function appInitializerFactory(workspaceService: WorkspaceService, httpClient: HttpClient) {
  return async () => {
    init(workspaceService, httpClient, environment.baseUrl, environment.authUrl);
  };
}

export function appBootstrapListenerFactory(dialogInfo: DialogInfoService, menuInfo: MenuInfoService, navigationInfo: NavigationInfoService, routeInfo: RouteInfoService) {
  return (component: ComponentRef<any>) => {
    const allors: { [key: string]: unknown } = (component.location.nativeElement.allors ??= {});
    dialogInfo.write(allors);
    menuInfo.write(allors);
    navigationInfo.write(allors);
    routeInfo.write(allors);
  };
}

export const environment = {
  production: false,
  baseUrl: 'http://localhost:5000/allors/',
  authUrl: 'TestAuthentication/Token',
  providers: [
    DialogInfoService,
    MenuInfoService,
    NavigationInfoService,
    RouteInfoService,
    {
      provide: APP_INITIALIZER,
      useFactory: appInitializerFactory,
      deps: [WorkspaceService, HttpClient],
      multi: true,
    },
    {
      provide: APP_BOOTSTRAP_LISTENER,
      useFactory: appBootstrapListenerFactory,
      deps: [DialogInfoService, MenuInfoService, NavigationInfoService, RouteInfoService],
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
import 'zone.js/plugins/zone-error'; // Included with Angular CLI.
