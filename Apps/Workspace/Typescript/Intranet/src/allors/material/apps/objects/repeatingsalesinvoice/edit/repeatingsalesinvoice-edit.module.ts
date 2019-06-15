import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatOptionModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';


import { AllorsMaterialAutoCompleteModule } from '../../../../base/components/role/autocomplete';

import { AllorsMaterialChipsModule } from '../../../../base/components/role/chips';
import { AllorsMaterialDatepickerModule } from '../../../../base/components/role/datepicker';
import { AllorsMaterialFileModule } from '../../../../base/components/role/file';
import { AllorsMaterialFilesModule } from '../../../..//base/components/role/files';
import { AllorsMaterialInputModule } from '../../../../base/components/role/input';
import { AllorsMaterialLocalisedTextModule } from '../../../../base/components/role/localisedtext';
import { AllorsMaterialSelectModule } from '../../../../base/components/role/select';
import { AllorsMaterialSideNavToggleModule } from '../../../../base/components/sidenavtoggle';
import { AllorsMaterialSlideToggleModule } from '../../../../base/components/role/slidetoggle';
import { AllorsMaterialStaticModule } from '../../../../base/components/role/static';
import { AllorsMaterialTextAreaModule } from '../../../../base/components/role/textarea';

import { RepeatingSalesInvoiceEditComponent } from './repeatingsalesinvoice-edit.component';
export { RepeatingSalesInvoiceEditComponent } from './repeatingsalesinvoice-edit.component';

@NgModule({
  declarations: [
    RepeatingSalesInvoiceEditComponent,
  ],
  exports: [
    RepeatingSalesInvoiceEditComponent,
  ],
  imports: [
    AllorsMaterialAutoCompleteModule,
    AllorsMaterialChipsModule,
    AllorsMaterialDatepickerModule,
    AllorsMaterialFileModule,
    AllorsMaterialFilesModule,
    AllorsMaterialInputModule,
    AllorsMaterialLocalisedTextModule,
    AllorsMaterialSelectModule,
    AllorsMaterialSideNavToggleModule,
    AllorsMaterialSlideToggleModule,
    AllorsMaterialStaticModule,
    AllorsMaterialTextAreaModule,
    CommonModule,

    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatDatepickerModule,
    MatDialogModule,
    MatDividerModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatRadioModule,
    MatSelectModule,
    MatTabsModule,
    MatToolbarModule,
    MatTooltipModule,
    MatOptionModule,
    ReactiveFormsModule,
    RouterModule,
  ],
})
export class RepeatingSalesInvoiceEditModule { }