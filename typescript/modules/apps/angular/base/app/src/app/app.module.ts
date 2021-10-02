import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule, NoopAnimationsModule } from '@angular/platform-browser/animations';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE, NativeDateAdapter } from '@angular/material/core';
import { MAT_AUTOCOMPLETE_DEFAULT_OPTIONS } from '@angular/material/autocomplete';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { enGB } from 'date-fns/locale';

import { WorkspaceService } from '@allors/workspace/angular/core';
import { Configuration, Engine, PrototypeObjectFactory } from '@allors/workspace/adapters/system';
import { DatabaseConnection, ReactiveDatabaseClient } from '@allors/workspace/adapters/json/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/default';
import { M } from '@allors/workspace/meta/default';
import { ruleBuilder } from '@allors/workspace/derivations/base-custom';

import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSliderModule } from '@angular/material/slider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';

import { AngularClient } from '../allors/angular-client';
import { AuthorizationService } from './auth/authorization.service';
import { AppComponent } from './app.component';
import { environment } from '../environments/environment';

import {
  DateConfig,
  MediaConfig,
  AuthenticationConfig,
  AuthenticationInterceptor,
  AllorsFocusDirective,
  AllorsBarcodeDirective,
  AuthenticationServiceBase,
  DateServiceCore,
  MediaServiceCore,
  AllorsBarcodeServiceCore,
  AllorsFocusServiceCore,
  NavigationServiceCore,
  RefreshServiceCore,
  AuthenticationService,
  DateService,
  AllorsFocusService,
  RefreshService,
  AllorsBarcodeService,
  NavigationService,
  MediaService,
  AllorsMaterialDialogService,
  ObjectService,
  SaveService,
  AllorsMaterialSideNavService,
  AllorsMaterialAssociationAutoCompleteComponent,
  AllorsMaterialDialogComponent,
  AllorsMaterialErrorDialogComponent,
  AllorsMaterialFilterFieldDialogComponent,
  AllorsMaterialFilterFieldSearchComponent,
  AllorsMaterialFilterComponent,
  AllorsMaterialFooterComponent,
  AllorsMaterialFooterSaveCancelComponent,
  AllorsMaterialHeaderComponent,
  AllorsMaterialLauncherComponent,
  AllorsMaterialMediaComponent,
  AllorMediaPreviewComponent,
  AllorsMaterialAutocompleteComponent,
  AllorsMaterialCheckboxComponent,
  AllorsMaterialChipsComponent,
  AllorsMaterialDatepickerComponent,
  AllorsMaterialDatetimepickerComponent,
  AllorsMaterialFileComponent,
  AllorsMaterialFilesComponent,
  AllorsMaterialInputComponent,
  AllorsMaterialLocalisedMarkdownComponent,
  AllorsMaterialLocalisedTextComponent,
  AllorsMaterialMarkdownComponent,
  AllorsMaterialMonthpickerComponent,
  AllorsMaterialRadioGroupComponent,
  AllorsMaterialSelectComponent,
  AllorsMaterialSliderComponent,
  AllorsMaterialSlideToggleComponent,
  AllorsMaterialStaticComponent,
  AllorsMaterialTextareaComponent,
  AllorsMaterialScannerComponent,
  AllorsMaterialSideMenuComponent,
  AllorsMaterialSideNavToggleComponent,
  AllorsMaterialTableComponent,
  FactoryFabComponent,
  AllorsMaterialDialogServiceCore,
  ObjectServiceCore,
  SaveServiceCore,
  AllorsMaterialSideNavServiceCore,
} from '@allors/workspace/angular/base';

import { LoginComponent } from './auth/login.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { OrganisationOverviewComponent } from './relations/organisations/organisation/organisation-overview.component';
import { OrganisationComponent } from './relations/organisations/organisation/organisation.component';
import { OrganisationsComponent } from './relations/organisations/organisations.component';
import { PersonOverviewComponent } from './relations/people/person/person-overview.component';
import { PersonComponent } from './relations/people/person/person.component';
import { PeopleComponent } from './relations/people/people.component';
import { FormComponent } from './tests/form/form.component';

import { configure } from './configure';
import { BaseContext } from '../allors/base-context';

export function appInitFactory(workspaceService: WorkspaceService, httpClient: HttpClient) {
  return async () => {
    const angularClient = new AngularClient(httpClient, environment.baseUrl, environment.authUrl);
    const client = new ReactiveDatabaseClient(angularClient);
    workspaceService.client = client;
    workspaceService.contextBuilder = (session) => new BaseContext(session, client);

    const metaPopulation = new LazyMetaPopulation(data);
    const m = metaPopulation as unknown as M;

    let nextId = -1;

    const configuration: Configuration = {
      name: 'Default',
      metaPopulation,
      objectFactory: new PrototypeObjectFactory(metaPopulation),
      idGenerator: () => nextId--,
      engine: new Engine(ruleBuilder(m)),
    };

    const database = new DatabaseConnection(configuration);
    const workspace = database.createWorkspace();
    workspaceService.workspace = workspace;

    configure(m);
  };
}

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
          { path: 'organisations', component: OrganisationsComponent },
          { path: 'organisation/:id', component: OrganisationOverviewComponent },
          { path: 'addorganisation', component: OrganisationComponent },
          { path: 'editorganisation/:id', component: OrganisationComponent },
          { path: 'people', component: PeopleComponent },
          { path: 'person/:id', component: PersonOverviewComponent },
          { path: 'addperson', component: PersonComponent },
          { path: 'editperson/:id', component: PersonComponent },
        ],
      },
      {
        path: 'tests',
        children: [
          {
            path: 'form',
            component: FormComponent,
          },
        ],
      },
    ],
  },
];

@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    // Allors Angular Core
    AllorsFocusDirective,
    AllorsBarcodeDirective,
    // Allors Angular Material Core
    AllorsMaterialAssociationAutoCompleteComponent,
    AllorsMaterialDialogComponent,
    AllorsMaterialErrorDialogComponent,
    AllorsMaterialFilterComponent,
    AllorsMaterialFilterFieldDialogComponent,
    AllorsMaterialFilterFieldSearchComponent,
    AllorsMaterialFooterComponent,
    AllorsMaterialFooterSaveCancelComponent,
    AllorsMaterialHeaderComponent,
    AllorsMaterialLauncherComponent,
    AllorsMaterialMediaComponent,
    AllorMediaPreviewComponent,
    AllorsMaterialAutocompleteComponent,
    AllorsMaterialCheckboxComponent,
    AllorsMaterialChipsComponent,
    AllorsMaterialDatepickerComponent,
    AllorsMaterialDatetimepickerComponent,
    AllorsMaterialFileComponent,
    AllorsMaterialFilesComponent,
    AllorsMaterialInputComponent,
    AllorsMaterialLocalisedMarkdownComponent,
    AllorsMaterialLocalisedTextComponent,
    AllorsMaterialMarkdownComponent,
    AllorsMaterialMonthpickerComponent,
    AllorsMaterialRadioGroupComponent,
    AllorsMaterialSelectComponent,
    AllorsMaterialSliderComponent,
    AllorsMaterialSlideToggleComponent,
    AllorsMaterialStaticComponent,
    AllorsMaterialTextareaComponent,
    AllorsMaterialScannerComponent,
    AllorsMaterialSideMenuComponent,
    AllorsMaterialSideNavToggleComponent,
    AllorsMaterialTableComponent,
    FactoryFabComponent,
    // Allors Angular Material Custom
    LoginComponent,
    MainComponent,
    DashboardComponent,
    OrganisationOverviewComponent,
    OrganisationComponent,
    OrganisationsComponent,
    PersonOverviewComponent,
    PersonComponent,
    PeopleComponent,
    FormComponent,
    // App
    AppComponent,
  ],
  imports: [
    BrowserModule,
    environment.production ? BrowserAnimationsModule : NoopAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' }),
    MatAutocompleteModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatPaginatorModule,
    MatRadioModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatStepperModule,
    MatTableModule,
    MatToolbarModule,
  ],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [WorkspaceService, HttpClient],
      multi: true,
    },
    WorkspaceService,
    { provide: AuthenticationService, useClass: AuthenticationServiceBase },
    {
      provide: AuthenticationConfig,
      useValue: {
        url: environment.baseUrl + environment.authUrl,
      },
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthenticationInterceptor,
      multi: true,
    },
    { provide: AllorsBarcodeService, useClass: AllorsBarcodeServiceCore },
    { provide: DateService, useClass: DateServiceCore },
    {
      provide: DateConfig,
      useValue: {
        locale: enGB,
      },
    },
    { provide: AllorsFocusService, useClass: AllorsFocusServiceCore },
    { provide: MediaService, useClass: MediaServiceCore },
    { provide: MediaConfig, useValue: { url: environment.baseUrl } },
    { provide: NavigationService, useClass: NavigationServiceCore },
    { provide: RefreshService, useClass: RefreshServiceCore },

    // Angular Material
    {
      provide: MAT_AUTOCOMPLETE_DEFAULT_OPTIONS,
      useValue: { autoActiveFirstOption: true },
    },
    { provide: DateAdapter, useClass: NativeDateAdapter },
    { provide: MAT_DATE_LOCALE, useValue: 'nl-BE' },
    {
      provide: MAT_DATE_FORMATS,
      useValue: {
        parse: {
          dateInput: 'dd/MM/yyyy',
        },
        display: {
          dateInput: 'dd/MM/yyyy',
          monthYearLabel: 'LLL y',
          dateA11yLabel: 'MMMM d, y',
          monthYearA11yLabel: 'MMMM y',
        },
      },
    },
    { provide: AllorsMaterialDialogService, useClass: AllorsMaterialDialogServiceCore },
    { provide: ObjectService, useClass: ObjectServiceCore },
    { provide: SaveService, useClass: SaveServiceCore },
    { provide: AllorsMaterialSideNavService, useClass: AllorsMaterialSideNavServiceCore },
  ],
})
export class AppModule {}
