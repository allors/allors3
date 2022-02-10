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
  CustomerShipmentOverviewComponent,
  ExchangeRateListComponent,
  GoodListComponent,
  NonUnifiedGoodOverviewComponent,
  NonUnifiedPartOverviewComponent,
  OrganisationListComponent,
  OrganisationOverviewComponent,
  PartListComponent,
  PersonListComponent,
  PersonOverviewComponent,
  PositionTypeRatesOverviewComponent,
  PositionTypesOverviewComponent,
  ProductCategoryListComponent,
  ProductQuoteListComponent,
  ProductQuoteOverviewComponent,
  ProductTypesOverviewComponent,
  PurchaseInvoiceListComponent,
  PurchaseInvoiceOverviewComponent,
  PurchaseOrderListComponent,
  PurchaseOrderOverviewComponent,
  PurchaseShipmentOverviewComponent,
  RequestForQuoteListComponent,
  RequestForQuoteOverviewComponent,
  SalesInvoiceListComponent,
  SalesInvoiceOverviewComponent,
  SalesOrderListComponent,
  SalesOrderOverviewComponent,
  SerialisedItemCharacteristicListComponent,
  SerialisedItemListComponent,
  SerialisedItemOverviewComponent,
  ShipmentListComponent,
  TaskAssignmentListComponent,
  UnifiedGoodListComponent,
  UnifiedGoodOverviewComponent,
  WorkEffortListComponent,
  WorkRequirementListComponent,
  WorkRequirementOverviewComponent,
  WorkTaskOverviewComponent,
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
          { path: 'person/:id', component: PersonOverviewComponent },
          { path: 'organisations', component: OrganisationListComponent },
          {
            path: 'organisation/:id',
            component: OrganisationOverviewComponent,
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
            component: RequestForQuoteOverviewComponent,
          },
          { path: 'productquotes', component: ProductQuoteListComponent },
          {
            path: 'productquote/:id',
            component: ProductQuoteOverviewComponent,
          },
          { path: 'salesorders', component: SalesOrderListComponent },
          { path: 'salesorder/:id', component: SalesOrderOverviewComponent },
          { path: 'salesinvoices', component: SalesInvoiceListComponent },
          {
            path: 'salesinvoice/:id',
            component: SalesInvoiceOverviewComponent,
          },
        ],
      },

      {
        path: 'products',
        children: [
          { path: 'goods', component: GoodListComponent },
          {
            path: 'nonunifiedgood/:id',
            component: NonUnifiedGoodOverviewComponent,
          },
          { path: 'parts', component: PartListComponent },
          {
            path: 'nonunifiedpart/:id',
            component: NonUnifiedPartOverviewComponent,
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
          { path: 'producttypes', component: ProductTypesOverviewComponent },
          { path: 'serialiseditems', component: SerialisedItemListComponent },
          {
            path: 'serialisedItem/:id',
            component: SerialisedItemOverviewComponent,
          },
          { path: 'unifiedgoods', component: UnifiedGoodListComponent },
          { path: 'unifiedgood/:id', component: UnifiedGoodOverviewComponent },
        ],
      },

      {
        path: 'purchasing',
        children: [
          { path: 'purchaseorders', component: PurchaseOrderListComponent },
          {
            path: 'purchaseorder/:id',
            component: PurchaseOrderOverviewComponent,
          },
          { path: 'purchaseinvoices', component: PurchaseInvoiceListComponent },
          {
            path: 'purchaseinvoice/:id',
            component: PurchaseInvoiceOverviewComponent,
          },
        ],
      },

      {
        path: 'shipment',
        children: [
          { path: 'shipments', component: ShipmentListComponent },
          {
            path: 'customershipment/:id',
            component: CustomerShipmentOverviewComponent,
          },
          {
            path: 'purchaseshipment/:id',
            component: PurchaseShipmentOverviewComponent,
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
            component: WorkRequirementOverviewComponent,
          },
          { path: 'workefforts', component: WorkEffortListComponent },
          { path: 'worktask/:id', component: WorkTaskOverviewComponent },
        ],
      },

      {
        path: 'humanresource',
        children: [
          { path: 'positiontypes', component: PositionTypesOverviewComponent },
          {
            path: 'positiontyperates',
            component: PositionTypeRatesOverviewComponent,
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
  CustomerShipmentOverviewComponent,
  ExchangeRateListComponent,
  GoodListComponent,
  NonUnifiedGoodOverviewComponent,
  NonUnifiedPartOverviewComponent,
  OrganisationListComponent,
  OrganisationOverviewComponent,
  PartListComponent,
  PersonListComponent,
  PersonOverviewComponent,
  PositionTypeRatesOverviewComponent,
  PositionTypesOverviewComponent,
  ProductCategoryListComponent,
  ProductQuoteListComponent,
  ProductQuoteOverviewComponent,
  ProductTypesOverviewComponent,
  PurchaseInvoiceListComponent,
  PurchaseInvoiceOverviewComponent,
  PurchaseOrderListComponent,
  PurchaseOrderOverviewComponent,
  PurchaseShipmentOverviewComponent,
  RequestForQuoteListComponent,
  RequestForQuoteOverviewComponent,
  SalesInvoiceListComponent,
  SalesInvoiceOverviewComponent,
  SalesOrderListComponent,
  SalesOrderOverviewComponent,
  SerialisedItemCharacteristicListComponent,
  SerialisedItemListComponent,
  SerialisedItemOverviewComponent,
  ShipmentListComponent,
  TaskAssignmentListComponent,
  UnifiedGoodListComponent,
  UnifiedGoodOverviewComponent,
  WorkEffortListComponent,
  WorkRequirementListComponent,
  WorkRequirementOverviewComponent,
  WorkTaskOverviewComponent,
];
