import { Routes } from '@angular/router';

import { AuthorizationService } from './auth/authorization.service';

import { LoginComponent } from './auth/login.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { FieldsComponent } from './fields/fields.component';

import { CountryListComponent } from './domain/country/list/country-list.component';
import { OrganisationListComponent } from './domain/organisation/list/organisation-list.component';
import { OrganisationItemPageComponent } from './domain/organisation/item/organisation-item-page.component';
import { PersonListComponent } from './domain/person/list/person-list.component';
import { PersonItemPageComponent } from './domain/person/item/person-item-page.component';

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
          { path: 'person/:id', component: PersonItemPageComponent },
          { path: 'organisations', component: OrganisationListComponent },
          {
            path: 'organisation/:id',
            component: OrganisationItemPageComponent,
          },
          { path: 'countries', component: CountryListComponent },
        ],
      },
      {
        path: 'fields',
        component: FieldsComponent,
      },
    ],
  },
];

export const components: any[] = [
  LoginComponent,
  MainComponent,
  DashboardComponent,
  FieldsComponent,
  CountryListComponent,
  OrganisationListComponent,
  OrganisationItemPageComponent,
  PersonListComponent,
  PersonItemPageComponent,
];
