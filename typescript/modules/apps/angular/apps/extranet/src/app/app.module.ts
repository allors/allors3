import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule, NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { MatLuxonDateModule } from '@angular/material-luxon-adapter';
import { MAT_AUTOCOMPLETE_DEFAULT_OPTIONS } from '@angular/material/autocomplete';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { enGB } from 'date-fns/locale';

import { WorkspaceService } from '@allors/workspace/angular/core';
import { PrototypeObjectFactory } from '@allors/workspace/adapters/system';
import { DatabaseConnection } from '@allors/workspace/adapters/json/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/default';
import { M, tags } from '@allors/workspace/meta/default';
import { ruleBuilder } from '@allors/workspace/derivations/default';

import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
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
import { MatTabsModule } from '@angular/material/tabs';
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
  OBJECT_CREATE_TOKEN,
  OBJECT_EDIT_TOKEN,
} from '@allors/workspace/angular/base';

// Angular Material Base
import {
  WorkEffortListComponent,
  WorkTaskCreateComponent,
  WorkTaskOverviewComponent,
  WorkTaskOverviewDetailComponent,
  WorkTaskOverviewSummaryComponent,
  PrintService,
  PrintConfig,
} from '@allors/workspace/angular/apps/extranet';

import { LoginComponent } from './auth/login.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';

import { ErrorComponent } from './error/error.component';
import { configure } from './configure';
import { ExtranetContext } from '../allors/extranet-context';
import { Configuration } from '@allors/workspace/domain/system';
import { applyRules } from '@allors/workspace/derivations/system';

export function appInitFactory(workspaceService: WorkspaceService, httpClient: HttpClient) {
  return async () => {
    const angularClient = new AngularClient(httpClient, environment.baseUrl, environment.authUrl);

    const metaPopulation = new LazyMetaPopulation(data);
    const m = metaPopulation as unknown as M;

    let nextId = -1;

    const configuration: Configuration = {
      name: 'Default',
      metaPopulation,
      objectFactory: new PrototypeObjectFactory(metaPopulation),
      idGenerator: () => nextId--,
    };

    const rules = ruleBuilder(m);
    applyRules(m, rules);

    const database = new DatabaseConnection(configuration, angularClient);
    const workspace = database.createWorkspace();

    workspaceService.workspace = workspace;
    workspaceService.contextBuilder = () => new ExtranetContext(workspaceService);

    configure(m);
  };
}

export const routes: Routes = [
  ...environment.routes,
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
          { path: 'workefforts', component: WorkEffortListComponent },
          { path: 'worktask/:id', component: WorkTaskOverviewComponent },
        ],
      },
    ],
  },
];

export const create = {
  [tags.WorkTask]: WorkTaskCreateComponent,
};

export const edit = {};

@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    // Allors Angular Core
    AllorsFocusDirective,
    AllorsBarcodeDirective,
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
    // Extranet
    WorkEffortListComponent,
    WorkTaskCreateComponent,
    WorkTaskOverviewComponent,
    WorkTaskOverviewDetailComponent,
    WorkTaskOverviewSummaryComponent,
    // App
    ErrorComponent,
    LoginComponent,
    MainComponent,
    DashboardComponent,
    AppComponent,
    ...environment.components,
  ],
  imports: [
    BrowserModule,
    environment.production ? BrowserAnimationsModule : NoopAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' }),
    MatLuxonDateModule,
    MatAutocompleteModule,
    MatBadgeModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDatepickerModule,
    MatDialogModule,
    MatExpansionModule,
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
    MatTabsModule,
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
    {
      provide: MAT_AUTOCOMPLETE_DEFAULT_OPTIONS,
      useValue: { autoActiveFirstOption: true },
    },
    { provide: MAT_DATE_LOCALE, useValue: 'nl-BE' },
    { provide: AllorsMaterialDialogService, useClass: AllorsMaterialDialogServiceCore },
    { provide: ObjectService, useClass: ObjectServiceCore },
    { provide: SaveService, useClass: SaveServiceCore },
    { provide: AllorsMaterialSideNavService, useClass: AllorsMaterialSideNavServiceCore },
    PrintService,
    { provide: PrintConfig, useValue: { url: environment.baseUrl } },
    { provide: ObjectService, useClass: ObjectServiceCore },
    { provide: OBJECT_CREATE_TOKEN, useValue: create },
    { provide: OBJECT_EDIT_TOKEN, useValue: edit },
  ],
})
export class AppModule {}
