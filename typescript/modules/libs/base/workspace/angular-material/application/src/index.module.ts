// dummy module for IDE support
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

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

import { AllorsMaterialDialogComponent } from '@allors/base/workspace/angular-material/foundation';

import {
  AllorsFocusDirective,
  AllorsBarcodeDirective,
} from '@allors/base/workspace/angular/foundation';

import {
  AllorsMaterialAssociationAutoCompleteComponent,
  AllorsMaterialErrorDialogComponent,
  AllorsMaterialFilterFieldDialogComponent,
  AllorsMaterialFilterFieldSearchComponent,
  AllorsMaterialFilterComponent,
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
} from './index';

@NgModule({
  declarations: [
    AllorsFocusDirective,
    AllorsBarcodeDirective,
    AllorsMaterialDialogComponent,
    AllorsMaterialAssociationAutoCompleteComponent,
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
  ],
  imports: [
    BrowserModule,
    NoopAnimationsModule,
    RouterModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
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
})
export class AppModule {}
