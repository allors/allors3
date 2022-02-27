import {
  APP_BOOTSTRAP_LISTENER,
  APP_INITIALIZER,
  ComponentRef,
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  CreateService,
  EditDialogService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { RouteInfoService } from '@allors/base/workspace/angular/foundation';
import {
  MenuInfoService,
  NavigationInfoService,
} from '@allors/base/workspace/angular/application';
import { dialogs } from '../app/app.dialog';
import { config } from '../app/app.config';

// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export function appInitializerFactory(
  workspaceService: WorkspaceService,
  httpClient: HttpClient,
  createService: AllorsMaterialCreateService,
  editService: AllorsMaterialEditDialogService
) {
  return async () => {
    config(
      workspaceService,
      httpClient,
      environment.baseUrl,
      environment.authUrl
    );

    createService.createControlByObjectTypeTag = dialogs.create;
    editService.editControlByObjectTypeTag = dialogs.edit;
  };
}

export function appBootstrapListenerFactory(
  menuInfo: MenuInfoService,
  navigationInfo: NavigationInfoService,
  routeInfo: RouteInfoService
) {
  return (component: ComponentRef<any>) => {
    const allors: { [key: string]: unknown } =
      (component.location.nativeElement.allors ??= {});
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
    MenuInfoService,
    NavigationInfoService,
    RouteInfoService,
    {
      provide: APP_INITIALIZER,
      useFactory: appInitializerFactory,
      deps: [WorkspaceService, HttpClient, CreateService, EditDialogService],
      multi: true,
    },
    {
      provide: APP_BOOTSTRAP_LISTENER,
      useFactory: appBootstrapListenerFactory,
      deps: [MenuInfoService, NavigationInfoService, RouteInfoService],
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
import 'zone.js/plugins/zone-error'; // Included with Angular CLI.import { AllorsMaterialCreateService } from '@allors/base/workspace/angular-material/application';
import {
  AllorsMaterialCreateService,
  AllorsMaterialEditDialogService,
} from '@allors/base/workspace/angular-material/application';
