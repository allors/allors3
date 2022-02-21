import { Routes } from '@angular/router';
import {
  WorkEffortListPageComponent,
  WorkTaskOverviewPageComponent,
} from '@allors/apps-extranet/workspace/angular-material';

import { AuthorizationService } from './auth/authorization.service';

import { LoginComponent } from './auth/login.component';
import { ErrorComponent } from './error/error.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'error', component: ErrorComponent },
  {
    canActivate: [AuthorizationService],
    path: '',
    component: MainComponent,
    children: [
      {
        path: '',
        component: DashboardComponent,
        pathMatch: 'full',
      },

      {
        path: 'workefforts',
        children: [
          { path: 'workefforts', component: WorkEffortListPageComponent },
          { path: 'worktask/:id', component: WorkTaskOverviewPageComponent },
        ],
      },
    ],
  },
];

export const components: any[] = [
  LoginComponent,
  ErrorComponent,
  MainComponent,
  DashboardComponent,
  WorkEffortListPageComponent,
  WorkTaskOverviewPageComponent,
];
