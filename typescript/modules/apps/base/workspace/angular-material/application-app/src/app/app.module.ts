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

import {
  CreateService,
  DisplayService,
  EditService,
  FilterService,
  FormService,
  MetaService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

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
  TemplateHostDirective,
} from '@allors/base/workspace/angular/foundation';

import {
  NavigationService,
  MenuService,
} from '@allors/base/workspace/angular/application';

import {
  AllorsMaterialDialogComponent,
  AllorsMaterialDialogService,
  AllorsMaterialPeriodSelectionToggleComponent,
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
  AllorsMaterialDynamicCreateComponent,
  AllorsMaterialDynamicEditComponent,
  AllorsMaterialDynamicEditDetailPanelComponent,
  AllorsMaterialDynamicViewDetailPanelComponent,
  AllorsMaterialDynamicEditRelationshipPanelComponent,
  AllorsMaterialDynamicViewRelationshipPanelComponent,
  AllorsMaterialErrorService,
  AllorsMaterialSideNavSubjectService,
  AllorsMaterialCreateService,
  AllorsMaterialEditService,
  SorterService,
  IconService,
} from '@allors/base/workspace/angular-material/application';

import { routes, components as routeComponents } from './app.routes';
import { components as dialogComponents } from './app.dialog';
import {
  AppFormService,
  components as formComponents,
} from './services/form.service';

import { OrganisationSummaryPanelComponent } from './domain/organisation/summary/organisation-summary-panel.component';
import { PersonInlineComponent } from './domain/person/inline/person-inline.component';
import { PersonFormComponent } from './domain/person/form/person-form.component';
import { PersonSummaryPanelComponent } from './domain/person/summary/person-summary-panel.component';
import { AppFilterService } from './services/filter.service';
import { AppSorterService } from './services/sorter.service';
import { AppMenuService } from './services/menu.service';
import { AppNavigationService } from './services/navigation.service';
import { AppIconService } from './services/icon.service';
import { AppMetaService } from './services/meta.service';
import { AppDisplayService } from './services/display.service';

@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    // Allors Angular Base
    AllorsFocusDirective,
    AllorsBarcodeDirective,
    // Allors Angular Material Base
    AllorsMaterialAssociationAutoCompleteComponent,
    AllorsMaterialDialogComponent,
    AllorsMaterialPeriodSelectionToggleComponent,
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
    AllorsMaterialDynamicCreateComponent,
    AllorsMaterialDynamicEditComponent,
    AllorsMaterialDynamicEditDetailPanelComponent,
    AllorsMaterialDynamicViewDetailPanelComponent,
    AllorsMaterialDynamicEditRelationshipPanelComponent,
    AllorsMaterialDynamicViewRelationshipPanelComponent,
    TemplateHostDirective,
    // Routed and dialog components
    ...routeComponents,
    ...dialogComponents,
    ...formComponents,
    // Non routed and non dialog components
    OrganisationSummaryPanelComponent,
    PersonInlineComponent,
    PersonFormComponent,
    PersonSummaryPanelComponent,
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

    // App Services
    { provide: FilterService, useClass: AppFilterService },
    { provide: FormService, useClass: AppFormService },
    { provide: SorterService, useClass: AppSorterService },
    { provide: MenuService, useClass: AppMenuService },
    { provide: NavigationService, useClass: AppNavigationService },
    { provide: IconService, useClass: AppIconService },
    { provide: MetaService, useClass: AppMetaService },
    { provide: DisplayService, useClass: AppDisplayService },

    ...environment.providers,
  ],
})
export class AppModule {}
