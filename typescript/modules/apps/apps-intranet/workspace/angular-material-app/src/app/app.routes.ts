import { Routes } from '@angular/router';
import { AuthorizationService } from './auth/authorization.service';

import { LoginComponent } from './auth/login.component';
import { ErrorComponent } from './error/error.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import {
  CarrierListComponent,
  CataloguesListComponent,
  CommunicationEventListComponent,
  CustomerShipmentOverviewPageComponent,
  ExchangeRateListComponent,
  GoodListComponent,
  NonUnifiedGoodOverviewPageComponent,
  NonUnifiedPartOverviewPageComponent,
  OrganisationListComponent,
  OrganisationOverviewPageComponent,
  PartListComponent,
  PersonListComponent,
  PersonOverviewPageComponent,
  PositionTypeRateListComponent,
  PositionTypesListComponent,
  ProductCategoryListComponent,
  ProductQuoteListComponent,
  ProductQuoteOverviewPageComponent,
  ProductTypesOverviewPageComponent,
  PurchaseInvoiceListComponent,
  PurchaseInvoiceOverviewPageComponent,
  PurchaseOrderListComponent,
  PurchaseOrderOverviewPageComponent,
  PurchaseShipmentOverviewPageComponent,
  RequestForQuoteListComponent,
  RequestForQuoteOverviewPageComponent,
  SalesInvoiceListComponent,
  SalesInvoiceOverviewPageComponent,
  SalesOrderListComponent,
  SalesOrderOverviewPageComponent,
  SerialisedItemCharacteristicListComponent,
  SerialisedItemListComponent,
  SerialisedItemOverviewPageComponent,
  ShipmentListComponent,
  TaskAssignmentListComponent,
  UnifiedGoodListComponent,
  UnifiedGoodOverviewPageComponent,
  WorkEffortListComponent,
  WorkRequirementListComponent,
  WorkRequirementOverviewPageComponent,
  WorkTaskOverviewPageComponent,
} from '@allors/apps-intranet/workspace/angular-material';

export const routes: Routes = [
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
        path: 'contacts',
        children: [
          { path: 'people', component: PersonListComponent },
          { path: 'person/:id', component: PersonOverviewPageComponent },
          { path: 'organisations', component: OrganisationListComponent },
          {
            path: 'organisation/:id',
            component: OrganisationOverviewPageComponent,
          },
          {
            path: 'communicationevents',
            component: CommunicationEventListComponent,
          },
        ],
      },

      {
        path: 'sales',
        children: [
          { path: 'requestsforquote', component: RequestForQuoteListComponent },
          {
            path: 'requestforquote/:id',
            component: RequestForQuoteOverviewPageComponent,
          },
          { path: 'productquotes', component: ProductQuoteListComponent },
          {
            path: 'productquote/:id',
            component: ProductQuoteOverviewPageComponent,
          },
          { path: 'salesorders', component: SalesOrderListComponent },
          {
            path: 'salesorder/:id',
            component: SalesOrderOverviewPageComponent,
          },
          { path: 'salesinvoices', component: SalesInvoiceListComponent },
          {
            path: 'salesinvoice/:id',
            component: SalesInvoiceOverviewPageComponent,
          },
        ],
      },

      {
        path: 'products',
        children: [
          { path: 'goods', component: GoodListComponent },
          {
            path: 'nonunifiedgood/:id',
            component: NonUnifiedGoodOverviewPageComponent,
          },
          { path: 'parts', component: PartListComponent },
          {
            path: 'nonunifiedpart/:id',
            component: NonUnifiedPartOverviewPageComponent,
          },
          { path: 'catalogues', component: CataloguesListComponent },
          {
            path: 'productcategories',
            component: ProductCategoryListComponent,
          },
          {
            path: 'serialiseditemcharacteristics',
            component: SerialisedItemCharacteristicListComponent,
          },
          {
            path: 'producttypes',
            component: ProductTypesOverviewPageComponent,
          },
          { path: 'serialiseditems', component: SerialisedItemListComponent },
          {
            path: 'serialisedItem/:id',
            component: SerialisedItemOverviewPageComponent,
          },
          { path: 'unifiedgoods', component: UnifiedGoodListComponent },
          {
            path: 'unifiedgood/:id',
            component: UnifiedGoodOverviewPageComponent,
          },
        ],
      },

      {
        path: 'purchasing',
        children: [
          { path: 'purchaseorders', component: PurchaseOrderListComponent },
          {
            path: 'purchaseorder/:id',
            component: PurchaseOrderOverviewPageComponent,
          },
          { path: 'purchaseinvoices', component: PurchaseInvoiceListComponent },
          {
            path: 'purchaseinvoice/:id',
            component: PurchaseInvoiceOverviewPageComponent,
          },
        ],
      },

      {
        path: 'shipment',
        children: [
          { path: 'shipments', component: ShipmentListComponent },
          {
            path: 'customershipment/:id',
            component: CustomerShipmentOverviewPageComponent,
          },
          {
            path: 'purchaseshipment/:id',
            component: PurchaseShipmentOverviewPageComponent,
          },
          { path: 'carriers', component: CarrierListComponent },
        ],
      },

      {
        path: 'workefforts',
        children: [
          { path: 'workrequirements', component: WorkRequirementListComponent },
          {
            path: 'workrequirement/:id',
            component: WorkRequirementOverviewPageComponent,
          },
          { path: 'workefforts', component: WorkEffortListComponent },
          { path: 'worktask/:id', component: WorkTaskOverviewPageComponent },
        ],
      },

      {
        path: 'humanresource',
        children: [
          {
            path: 'positiontypes',
            component: PositionTypesListComponent,
          },
          {
            path: 'positiontyperates',
            component: PositionTypeRateListComponent,
          },
        ],
      },

      {
        path: 'workflow',
        children: [
          { path: 'taskassignments', component: TaskAssignmentListComponent },
        ],
      },
      {
        path: 'accounting',
        children: [
          { path: 'exchangerates', component: ExchangeRateListComponent },
        ],
      },
    ],
  },
];

export const components: any[] = [
  LoginComponent,
  ErrorComponent,
  MainComponent,
  DashboardComponent,
  CarrierListComponent,
  CataloguesListComponent,
  CommunicationEventListComponent,
  CustomerShipmentOverviewPageComponent,
  ExchangeRateListComponent,
  GoodListComponent,
  NonUnifiedGoodOverviewPageComponent,
  NonUnifiedPartOverviewPageComponent,
  OrganisationListComponent,
  OrganisationOverviewPageComponent,
  PartListComponent,
  PersonListComponent,
  PersonOverviewPageComponent,
  PositionTypeRateListComponent,
  PositionTypesListComponent,
  ProductCategoryListComponent,
  ProductQuoteListComponent,
  ProductQuoteOverviewPageComponent,
  ProductTypesOverviewPageComponent,
  PurchaseInvoiceListComponent,
  PurchaseInvoiceOverviewPageComponent,
  PurchaseOrderListComponent,
  PurchaseOrderOverviewPageComponent,
  PurchaseShipmentOverviewPageComponent,
  RequestForQuoteListComponent,
  RequestForQuoteOverviewPageComponent,
  SalesInvoiceListComponent,
  SalesInvoiceOverviewPageComponent,
  SalesOrderListComponent,
  SalesOrderOverviewPageComponent,
  SerialisedItemCharacteristicListComponent,
  SerialisedItemListComponent,
  SerialisedItemOverviewPageComponent,
  ShipmentListComponent,
  TaskAssignmentListComponent,
  UnifiedGoodListComponent,
  UnifiedGoodOverviewPageComponent,
  WorkEffortListComponent,
  WorkRequirementListComponent,
  WorkRequirementOverviewPageComponent,
  WorkTaskOverviewPageComponent,
];
