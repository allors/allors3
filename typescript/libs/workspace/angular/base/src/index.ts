// Services
export { AuthenticationTokenRequest } from './lib/services/authentication/AuthenticationTokenRequest';
export { AuthenticationTokenResponse } from './lib/services/authentication/AuthenticationTokenResponse';
export { AuthenticationService } from './lib/services/authentication/authentication.service';
export { AllorsBarcodeService } from './lib/services/barcode/barcode.service';
export { DateService } from './lib/services/date/date.service';
export { AllorsFocusService } from './lib/services/focus/focus.service';
export { MediaService } from './lib/services/media/media.service';
export { NavigationService } from './lib/services/navigation/navigation.service';
export { PanelService } from './lib/services/panel/panel.service';
export { PanelManagerService } from './lib/services/panel/panelmanager.service';
export { RefreshService } from './lib/services/refresh/refresh.service';
export { SessionState } from './lib/services/state/SessionState';
export { SingletonId } from './lib/services/state/SingletonId';
export { UserId } from './lib/services/state/UserId';

// Components
export { Action } from './lib/components/actions/Action';
export { ActionResult } from './lib/components/actions/ActionResult';
export { ActionTarget } from './lib/components/actions/ActionTarget';

export { AuthenticationConfig } from './lib/components/authentication/authentication.config';
export { AuthenticationInterceptor } from './lib/components/authentication/authentication.interceptor';
export { AuthenticationServiceBase } from './lib/components/authentication/authentication.service.base';

export { AllorsBarcodeDirective } from './lib/components/barcode/barcode.directive';
export { AllorsBarcodeServiceCore } from './lib/components/barcode/barcode.service.core';

export { DateConfig } from './lib/components/date/date.config';
export { DateServiceCore } from './lib/components/date/date.service.core';

export { Filter } from './lib/components/filter/Filter';
export { FilterDefinition } from './lib/components/filter/FilterDefinition';
export { FilterField } from './lib/components/filter/FilterField';
export { FilterFieldDefinition } from './lib/components/filter/FilterFieldDefinition';
export { FilterOptions } from './lib/components/filter/FilterOptions';

export { AllorsFocusDirective } from './lib/components/focus/focus.directive';
export { AllorsFocusServiceCore } from './lib/components/focus/focus.service.core';

export { AssociationField } from './lib/components/forms/AssociationField';
export { Field } from './lib/components/forms/Field';
export { LocalisedRoleField } from './lib/components/forms/LocalisedRoleField';
export { ModelField } from './lib/components/forms/ModelField';
export { RoleField } from './lib/components/forms/RoleField';

export { MediaConfig } from './lib/components/media/media.config';
export { MediaServiceCore } from './lib/components/media/media.service.core';

export { NavigationServiceCore } from './lib/components/navigation/navigation.service.core';
export { NavigationActivatedRoute } from './lib/components/navigation/NavigationActivatedRoute';

export { RefreshServiceCore } from './lib/components/refresh/refresh.service.core';

export { SearchFactory } from './lib/components/search/SearchFactory';
export { SearchOptions } from './lib/components/search/SearchOptions';

export { TestScope } from './lib/components/test/test.scope';

// Material
export { DialogConfig, PromptType } from './lib/material/services/dialog/dialog.config';
export { AllorsMaterialDialogService } from './lib/material/services/dialog/dialog.service';

export { ObjectData } from './lib/material/services/object/object.data';
export { ObjectService } from './lib/material/services/object/object.service';
export { OBJECT_CREATE_TOKEN, OBJECT_EDIT_TOKEN } from './lib/material/services/object/object.tokens';

export { SaveService } from './lib/material/services/save/save.service';

export { AllorsMaterialSideNavService } from './lib/material/services/sidenav/sidenav.service';

export { AllorsMaterialAssociationAutoCompleteComponent } from './lib/material/components/association/autocomplete/autocomplete.component';

export { AllorsMaterialDialogComponent } from './lib/material/components/dialog/dialog.component';
export { FactoryFabComponent } from './lib/material/components/factoryfab/factoryfab.component';

export { AllorsMaterialFilterFieldDialogComponent } from './lib/material/components/filter/field/dialog.component';
export { AllorsMaterialFilterFieldSearchComponent } from './lib/material/components/filter/field/search.component';
export { AllorsMaterialFilterComponent } from './lib/material/components/filter/filter.component';

export { AllorsMaterialFooterSaveCancelComponent } from './lib/material/components/footer/savecancel/savecancel.component';
export { AllorsMaterialFooterComponent } from './lib/material/components/footer/footer.component';

export { AllorsMaterialHeaderComponent } from './lib/material/components/header/header.component';
export { AllorsMaterialLauncherComponent } from './lib/material/components/launcher/launcher.component';

export { MediaDialogData } from './lib/material/components/media/preview/dialog.data';
export { AllorMediaPreviewComponent } from './lib/material/components/media/preview/media-preview.component';
export { AllorsMaterialMediaComponent } from './lib/material/components/media/media.component';

export { AllorsMaterialAutocompleteComponent } from './lib/material/components/role/autocomplete/autocomplete.component';
export { AllorsMaterialCheckboxComponent } from './lib/material/components/role/checkbox/checkbox.component';
export { AllorsMaterialChipsComponent } from './lib/material/components/role/chips/chips.component';
export { AllorsMaterialDatepickerComponent } from './lib/material/components/role/datepicker/datepicker.component';
export { AllorsMaterialDatetimepickerComponent } from './lib/material/components/role/datetimepicker/datetimepicker.component';
export { AllorsMaterialFileComponent } from './lib/material/components/role/file/file.component';
export { AllorsMaterialFilesComponent } from './lib/material/components/role/files/files.component';
export { AllorsMaterialInputComponent } from './lib/material/components/role/input/input.component';
export { AllorsMaterialLocalisedMarkdownComponent } from './lib/material/components/role/localisedmarkdown/localisedmarkdown.component';
export { AllorsMaterialLocalisedTextComponent } from './lib/material/components/role/localisedtext/localisedtext.component';
export { AllorsMaterialMarkdownComponent } from './lib/material/components/role/markdown/markdown.component';
export { AllorsMaterialMonthpickerComponent } from './lib/material/components/role/monthpicker/monthpicker.component';
export { AllorsMaterialRadioGroupComponent, RadioGroupOption } from './lib/material/components/role/radiogroup/radiogroup.component';
export { AllorsMaterialSelectComponent } from './lib/material/components/role/select/select.component';
export { AllorsMaterialSliderComponent } from './lib/material/components/role/slider/slider.component';
export { AllorsMaterialSlideToggleComponent } from './lib/material/components/role/slidetoggle/slidetoggle.component';
export { AllorsMaterialStaticComponent } from './lib/material/components/role/static/static.component';
export { AllorsMaterialTextareaComponent } from './lib/material/components/role/textarea/textarea.component';

export { AllorsMaterialScannerComponent } from './lib/material/components/scanner/scanner.component';
export { AllorsMaterialSideMenuComponent } from './lib/material/components/sidemenu/sidemenu.component';
export { SideMenuItem } from './lib/material/components/sidemenu/sidemenuitem';
export { AllorsMaterialSideNavToggleComponent } from './lib/material/components/sidenavtoggle/sidenavtoggle.component';

export { BaseTable } from './lib/material/components/table/BaseTable';
export { Column } from './lib/material/components/table/Column';
export { Table } from './lib/material/components/table/Table';
export { TableConfig } from './lib/material/components/table/TableConfig';
export { TableRow } from './lib/material/components/table/TableRow';
export { AllorsMaterialTableComponent } from './lib/material/components/table/table.component';

export { AllorsDateAdapter as AllorsDateAdapter } from './lib/material/dateadapter/allorsdateadapter';

export { DeleteService } from './lib/material/services/actions/delete/delete.service';
export { DeleteAction } from './lib/material/services/actions/delete/DeleteAction';
export { EditService } from './lib/material/services/actions/edit/edit.service';
export { EditAction } from './lib/material/services/actions/edit/EditAction';
export { MethodService } from './lib/material/services/actions/method/method.service';
export { MethodAction } from './lib/material/services/actions/method/MethodAction';
export { MethodConfig } from './lib/material/services/actions/method/MethodConfig';
export { OverviewService } from './lib/material/services/actions/overview/overview.service';
export { OverviewAction } from './lib/material/services/actions/overview/OverviewAction';

export { AllorsMaterialDialogServiceCore } from './lib/material/services/dialog/dialog.service.core';
export { ObjectServiceCore } from './lib/material/services/object/object.service.core';
export { SaveServiceCore } from './lib/material/services/save/save.service.core';
export { AllorsMaterialErrorDialogComponent } from './lib/material/services/save/error/errordialog.component';
export { AllorsMaterialSideNavServiceCore } from './lib/material/services/sidenav/sidenav.service.core';

export { Sorter } from './lib/material/sorting/Sorter';

export { IAngularComposite } from './lib/meta/IAngularComposite';
export { IAngularMetaService, IAngularMetaObject } from './lib/meta/IAngularMetaService';
export { IAngularRoleType } from './lib/meta/IAngularRoleType';

import { IAngularMetaService } from './lib/meta/IAngularMetaService';

declare module '@allors/workspace/domain/system' {
  interface IWorkspaceServices {
    angularMetaService: IAngularMetaService;
  }
}
