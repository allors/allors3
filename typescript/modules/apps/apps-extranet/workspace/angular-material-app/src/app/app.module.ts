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

import {
  // Config
  DateConfig,
  MediaConfig,
  AuthenticationConfig,
  // Services
  CreateService,
  DisplayService,
  EditDialogService,
  FilterService,
  FormService,
  MetaService,
  WorkspaceService,
  ErrorService,
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
  // Interceptors
  AuthenticationInterceptor,
  // Directives
  AllorsFocusDirective,
  AllorsBarcodeDirective,
  ActionService,
  TemplateHostDirective,
  ThrottledDirective,
} from '@allors/base/workspace/angular/foundation';

import {
  // Services
  NavigationService,
  MenuService,
} from '@allors/base/workspace/angular/application';

import {
  // Components
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
  AllorsMaterialPaginatorComponent,
  AllorsMaterialRadioGroupComponent,
  AllorsMaterialSelectComponent,
  AllorsMaterialSliderComponent,
  AllorsMaterialSlideToggleComponent,
  AllorsMaterialStaticComponent,
  AllorsMaterialTextareaComponent,
  AllorsMaterialTableComponent,
} from '@allors/base/workspace/angular-material/foundation';

import {
  // Services
  AllorsMaterialSideNavService,
  AllorsMaterialErrorService,
  AllorsMaterialSideNavSubjectService,
  AllorsMaterialCreateService,
  AllorsMaterialEditDialogService,
  SorterService,
  IconService,
  HyperlinkService,
  // Components
  AllorsMaterialErrorDialogComponent,
  AllorsMaterialFilterFieldDialogComponent,
  AllorsMaterialFilterFieldSearchComponent,
  AllorsMaterialFilterComponent,
  AllorsMaterialMediaComponent,
  AllorMediaPreviewComponent,
  AllorsMaterialBarcodeEntryComponent,
  AllorsMaterialSideMenuComponent,
  AllorsMaterialSideNavToggleComponent,
  FactoryFabComponent,
  AllorsMaterialDynamicCreateComponent,
  AllorsMaterialDynamicEditComponent,
  AllorsMaterialDynamicEditDetailPanelComponent,
  AllorsMaterialDynamicEditExtentPanelComponent,
  AllorsMaterialDynamicSummaryPanelComponent,
  AllorsMaterialDynamicViewDetailPanelComponent,
  AllorsMaterialDynamicViewExtentPanelComponent,
} from '@allors/base/workspace/angular-material/application';

import {
  // Components
  WorkEffortListPageComponent,
  WorkTaskCreateFormComponent,
  WorkTaskOverviewPageComponent,
  WorkTaskEditFormComponent,
  WorkTaskSummaryPanel,
} from '@allors/apps-extranet/workspace/angular-material';

import { routes, components as routeComponents } from './app.routes';
import { components as dialogComponents } from './app.dialog';
import {
  AppFormService,
  components as formComponents,
} from './services/form.service';

import { AppFilterService } from './services/filter.service';
import { AppSorterService } from './services/sorter.service';
import { AppMenuService } from './services/menu.service';
import { AppNavigationService } from './services/navigation.service';
import { AppIconService } from './services/icon.service';
import { AppMetaService } from './services/meta.service';
import { AppDisplayService } from './services/display.service';

import { AppComponent } from './app.component';
import { environment } from '../environments/environment';
import { AppHyperlinkService } from './services/hyperlink.service';
import { AppActionService } from './services/action.service';

@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    // Base Foundation
    AllorsFocusDirective,
    AllorsBarcodeDirective,
    TemplateHostDirective,
    ThrottledDirective,
    // Base Material Foundation
    AllorsMaterialAssociationAutoCompleteComponent,
    AllorsMaterialCancelComponent,
    AllorsMaterialDialogComponent,
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
    AllorsMaterialPaginatorComponent,
    AllorsMaterialRadioGroupComponent,
    AllorsMaterialSelectComponent,
    AllorsMaterialSliderComponent,
    AllorsMaterialSlideToggleComponent,
    AllorsMaterialStaticComponent,
    AllorsMaterialTextareaComponent,
    AllorsMaterialPeriodSelectionToggleComponent,
    AllorsMaterialTableComponent,
    // Base Material Application
    AllorsMaterialErrorDialogComponent,
    AllorsMaterialFilterFieldDialogComponent,
    AllorsMaterialFilterFieldSearchComponent,
    AllorsMaterialFilterComponent,
    AllorsMaterialMediaComponent,
    AllorMediaPreviewComponent,
    AllorsMaterialBarcodeEntryComponent,
    AllorsMaterialSideMenuComponent,
    AllorsMaterialSideNavToggleComponent,
    FactoryFabComponent,
    AllorsMaterialDynamicCreateComponent,
    AllorsMaterialDynamicEditComponent,
    AllorsMaterialDynamicEditDetailPanelComponent,
    AllorsMaterialDynamicEditExtentPanelComponent,
    AllorsMaterialDynamicSummaryPanelComponent,
    AllorsMaterialDynamicViewDetailPanelComponent,
    AllorsMaterialDynamicViewExtentPanelComponent,
    // Routed and dialog components
    ...routeComponents,
    ...dialogComponents,
    ...formComponents,
    // Non routed and non dialog components
    WorkEffortListPageComponent,
    WorkTaskCreateFormComponent,
    WorkTaskOverviewPageComponent,
    WorkTaskEditFormComponent,
    WorkTaskSummaryPanel,
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
    { provide: MAT_DATE_LOCALE, useValue: 'nl' },
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
    {
      provide: AllorsMaterialEditDialogService,
      useClass: AllorsMaterialEditDialogService,
    },
    {
      provide: EditDialogService,
      useExisting: AllorsMaterialEditDialogService,
    },

    // App Services
    { provide: FilterService, useClass: AppFilterService },
    { provide: FormService, useClass: AppFormService },
    { provide: SorterService, useClass: AppSorterService },
    { provide: MenuService, useClass: AppMenuService },
    { provide: NavigationService, useClass: AppNavigationService },
    { provide: IconService, useClass: AppIconService },
    { provide: MetaService, useClass: AppMetaService },
    { provide: DisplayService, useClass: AppDisplayService },
    { provide: HyperlinkService, useClass: AppHyperlinkService },
    { provide: ActionService, useClass: AppActionService },

    ...environment.providers,
  ],
})
export class AppModule {}
