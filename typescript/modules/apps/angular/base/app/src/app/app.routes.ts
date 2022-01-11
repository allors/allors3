import { Routes } from '@angular/router';

import { AuthorizationService } from './auth/authorization.service';

import { LoginComponent } from './auth/login.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { FormComponent } from './form/form.component';

import { CountryListComponent } from './domain/country/list/country-list.component';
import { OrganisationListComponent } from './domain/organisation/list/organisation-list.component';
import { OrganisationOverviewComponent } from './domain/organisation/overview/organisation-overview.component';
import { PersonListComponent } from './domain/person/list/person-list.component';
import { PersonOverviewComponent } from './domain/person/overview/person-overview.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  {
    path: '',
    component: MainComponent,
    canActivate: [AuthorizationService],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
      },
      {
        path: 'contacts',
        children: [
          { path: 'people', component: PersonListComponent },
          { path: 'person/:id', component: PersonOverviewComponent },
          { path: 'organisations', component: OrganisationListComponent },
          {
            path: 'organisation/:id',
            component: OrganisationOverviewComponent,
          },
          { path: 'countries', component: CountryListComponent },
        ],
      },
      {
        path: 'form',
        component: FormComponent,
      },
    ],
  },
];

export const components: any[] = [
  LoginComponent,
  MainComponent,
  DashboardComponent,
  FormComponent,
  CountryListComponent,
  OrganisationListComponent,
  OrganisationOverviewComponent,
  PersonListComponent,
  PersonOverviewComponent,
];
