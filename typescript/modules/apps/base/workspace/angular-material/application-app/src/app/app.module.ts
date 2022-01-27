import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import {
  BrowserAnimationsModule,
  NoopAnimationsModule,
} from '@angular/platform-browser/animations';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { MatLuxonDateModule } from '@angular/material-luxon-adapter';
import { MAT_AUTOCOMPLETE_DEFAULT_OPTIONS } from '@angular/material/autocomplete';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { enGB } from 'date-fns/locale';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
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
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';

import { AppComponent } from './app.component';
import { environment } from '../environments/environment';

import {
  ErrorService,
  DateConfig,
  MediaConfig,
  AuthenticationConfig,
  AuthenticationInterceptor,
  AllorsFocusDirective,
  AllorsBarcodeDirective,
  AuthenticationSessionStoreService,
  DateStaticService,
  AllorsBarcodeKeypressService,
  AllorsFocusBehaviorSubjectService,
  RefreshBehaviorService,
  AuthenticationService,
  DateService,
  AllorsFocusService,
  RefreshService,
  AllorsBarcodeService,
  MediaService,
  MediaLocalService,
  AllorsDialogService,
  FormHostDirective,
} from '@allors/base/workspace/angular/foundation';

import {
  CreateService,
  EditService,
  NavigationService,
  NavigationMetaService,
} from '@allors/base/workspace/angular/application';

import {
  AllorsMaterialDialogComponent,
  AllorsMaterialDialogService,
  AllorsMaterialPeriodToggleComponent,
} from '@allors/base/workspace/angular-material/foundation';

import {
  AllorsMaterialAssociationAutoCompleteComponent,
  AllorsMaterialCancelComponent,
  AllorsMaterialSaveComponent,
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
  AllorsMaterialRadioGroupComponent,
  AllorsMaterialSelectComponent,
  AllorsMaterialSliderComponent,
  AllorsMaterialSlideToggleComponent,
  AllorsMaterialStaticComponent,
  AllorsMaterialTextareaComponent,
} from '@allors/base/workspace/angular-material/foundation';

import {
  AllorsMaterialSideNavService,
  AllorsMaterialErrorDialogComponent,
  AllorsMaterialFilterFieldDialogComponent,
  AllorsMaterialFilterFieldSearchComponent,
  AllorsMaterialFilterComponent,
  AllorsMaterialMediaComponent,
  AllorMediaPreviewComponent,
  AllorsMaterialBarcodeEntryComponent,
  AllorsMaterialSideMenuComponent,
  AllorsMaterialSideNavToggleComponent,
  AllorsMaterialTableComponent,
  FactoryFabComponent,
  DynamicCreateComponent,
  DynamicEditComponent,
  AllorsMaterialDynamicRelationshipPanelComponent,
  AllorsMaterialErrorService,
  AllorsMaterialSideNavSubjectService,
  AllorsMaterialCreateService,
  AllorsMaterialEditService,
} from '@allors/base/workspace/angular-material/application';
import { routes, components as routesComponents } from './app.routes';
import { components as dialogsComponents } from './app.dialogs';

import { OrganisationDetailComponent } from './domain/organisation/detail/organisation-detail.component';
import { OrganisationSummaryComponent } from './domain/organisation/summary/organisation-summary.component';
import { PersonInlineComponent } from './domain/person/inline/person-inline.component';
import { PersonDetailComponent } from './domain/person/detail/person-detail.component';
import { PersonSummaryComponent } from './domain/person/summary/person-summary.component';

import { CountryFormComponent } from './domain/country/forms/country-form.component';

@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    // Allors Angular Base
    AllorsFocusDirective,
    AllorsBarcodeDirective,
    // Allors Angular Material Base
    AllorsMaterialAssociationAutoCompleteComponent,
    AllorsMaterialDialogComponent,
    AllorsMaterialPeriodToggleComponent,
    AllorsMaterialErrorDialogComponent,
    AllorsMaterialFilterComponent,
    AllorsMaterialFilterFieldDialogComponent,
    AllorsMaterialFilterFieldSearchComponent,
    AllorsMaterialMediaComponent,
    AllorMediaPreviewComponent,
    AllorsMaterialCancelComponent,
    AllorsMaterialSaveComponent,
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
    AllorsMaterialRadioGroupComponent,
    AllorsMaterialSelectComponent,
    AllorsMaterialSliderComponent,
    AllorsMaterialSlideToggleComponent,
    AllorsMaterialStaticComponent,
    AllorsMaterialTextareaComponent,
    AllorsMaterialBarcodeEntryComponent,
    AllorsMaterialSideMenuComponent,
    AllorsMaterialSideNavToggleComponent,
    AllorsMaterialTableComponent,
    FactoryFabComponent,
    DynamicCreateComponent,
    DynamicEditComponent,
    AllorsMaterialDynamicRelationshipPanelComponent,
    FormHostDirective,
    // Routed and dialog components
    ...routesComponents,
    ...dialogsComponents,
    // Non routed and non dialog components
    OrganisationDetailComponent,
    OrganisationSummaryComponent,
    PersonInlineComponent,
    PersonDetailComponent,
    PersonSummaryComponent,
    // Forms
    CountryFormComponent,
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
    MatLuxonDateModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatButtonToggleModule,
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
    MatTabsModule,
    MatToolbarModule,
  ],
  providers: [
    WorkspaceService,
    {
      provide: AuthenticationService,
      useClass: AuthenticationSessionStoreService,
    },
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
    { provide: AllorsBarcodeService, useClass: AllorsBarcodeKeypressService },
    { provide: DateService, useClass: DateStaticService },
    {
      provide: DateConfig,
      useValue: {
        locale: enGB,
      },
    },
    {
      provide: AllorsFocusService,
      useClass: AllorsFocusBehaviorSubjectService,
    },
    { provide: MediaService, useClass: MediaLocalService },
    { provide: MediaConfig, useValue: { url: environment.baseUrl } },
    { provide: NavigationService, useClass: NavigationMetaService },
    { provide: RefreshService, useClass: RefreshBehaviorService },

    // Angular Material
    {
      provide: MAT_AUTOCOMPLETE_DEFAULT_OPTIONS,
      useValue: { autoActiveFirstOption: true },
    },
    { provide: MAT_DATE_LOCALE, useValue: 'nl-BE' },
    { provide: MatDialogRef, useValue: {} },
    {
      provide: AllorsDialogService,
      useClass: AllorsMaterialDialogService,
    },
    { provide: ErrorService, useClass: AllorsMaterialErrorService },
    {
      provide: AllorsMaterialSideNavService,
      useClass: AllorsMaterialSideNavSubjectService,
    },
    {
      provide: AllorsMaterialCreateService,
      useClass: AllorsMaterialCreateService,
    },
    { provide: CreateService, useExisting: AllorsMaterialCreateService },
    { provide: AllorsMaterialEditService, useClass: AllorsMaterialEditService },
    { provide: EditService, useExisting: AllorsMaterialEditService },
    ...environment.providers,
  ],
})
export class AppModule {}
