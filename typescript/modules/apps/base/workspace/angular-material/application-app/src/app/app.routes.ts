import { Routes } from '@angular/router';

import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { FieldsComponent } from './fields/fields-form.component';
import { FilterPageComponent } from './filter/filter-page.component';

import { CountryListPageComponent } from './domain/country/list/country-list-page.component';
import { OrganisationListPageComponent } from './domain/organisation/list/organisation-list-page.component';
import { OrganisationOverviewPageComponent } from './domain/organisation/overview/organisation-overview-page.component';
import { PersonListPageComponent } from './domain/person/list/person-list-page.component';
import { PersonOverviewPageComponent } from './domain/person/overview/person-overview-page.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  {
    path: '',
    component: MainComponent,
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
      },
      {
        path: 'contacts',
        children: [
          { path: 'people', component: PersonListPageComponent },
          { path: 'person/:id', component: PersonOverviewPageComponent },
          {
            path: 'organisations',
            component: OrganisationListPageComponent,
          },
          {
            path: 'organisation/:id',
            component: OrganisationOverviewPageComponent,
          },
          { path: 'countries', component: CountryListPageComponent },
        ],
      },
      {
        path: 'fields',
        component: FieldsComponent,
      },
      {
        path: 'filter',
        component: FilterPageComponent,
      },
    ],
  },
];

export const components: any[] = [
  MainComponent,
  DashboardComponent,
  FieldsComponent,
  FilterPageComponent,
  CountryListPageComponent,
  OrganisationListPageComponent,
  OrganisationOverviewPageComponent,
  PersonListPageComponent,
  PersonOverviewPageComponent,
];
