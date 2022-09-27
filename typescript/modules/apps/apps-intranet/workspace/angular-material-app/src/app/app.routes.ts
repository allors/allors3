import { Routes } from '@angular/router';
import { AuthorizationService } from './auth/authorization.service';

import { LoginComponent } from './auth/login.component';
import { ErrorComponent } from './error/error.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import {
  BrandListPageComponent,
  CarrierListPageComponent,
  CataloguesListPageComponent,
  CommunicationEventListPageComponent,
  CustomerShipmentOverviewPageComponent,
  EmailMessageListPageComponent,
  ExchangeRateListPageComponent,
  FacilityListPageComponent,
  GoodListPageComponent,
  ModelListPageComponent,
  NonUnifiedGoodOverviewPageComponent,
  NonUnifiedPartOverviewPageComponent,
  OrganisationListPageComponent,
  OrganisationOverviewPageComponent,
  PartListPageComponent,
  PersonListPageComponent,
  PersonOverviewPageComponent,
  PositionTypeRateListPageComponent,
  PositionTypesListPageComponent,
  ProductCategoryListPageComponent,
  ProductQuoteListPageComponent,
  ProductQuoteOverviewPageComponent,
  ProductTypesOverviewPageComponent,
  ProposalListPageComponent,
  ProposalOverviewPageComponent,
  PurchaseInvoiceListPageComponent,
  PurchaseInvoiceOverviewPageComponent,
  PurchaseOrderListPageComponent,
  PurchaseOrderOverviewPageComponent,
  PurchaseReturnOverviewPageComponent,
  PurchaseShipmentOverviewPageComponent,
  RequestForQuoteListPageComponent,
  RequestForQuoteOverviewPageComponent,
  SalesInvoiceListPageComponent,
  SalesInvoiceOverviewPageComponent,
  SalesOrderListPageComponent,
  SalesOrderOverviewPageComponent,
  SerialisedItemCharacteristicListPageComponent,
  SerialisedItemListPageComponent,
  SerialisedItemOverviewPageComponent,
  ShipmentListPageComponent,
  TaskAssignmentListPageComponent,
  UnifiedGoodListPageComponent,
  UnifiedGoodOverviewPageComponent,
  WorkEffortListPageComponent,
  WorkRequirementListPageComponent,
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
          { path: 'people', component: PersonListPageComponent },
          { path: 'person/:id', component: PersonOverviewPageComponent },
          { path: 'organisations', component: OrganisationListPageComponent },
          {
            path: 'organisation/:id',
            component: OrganisationOverviewPageComponent,
          },
          {
            path: 'communicationevents',
            component: CommunicationEventListPageComponent,
          },
        ],
      },

      {
        path: 'sales',
        children: [
          {
            path: 'requestsforquote',
            component: RequestForQuoteListPageComponent,
          },
          {
            path: 'requestforquote/:id',
            component: RequestForQuoteOverviewPageComponent,
          },
          { path: 'productquotes', component: ProductQuoteListPageComponent },
          {
            path: 'productquote/:id',
            component: ProductQuoteOverviewPageComponent,
          },
          { path: 'proposals', component: ProposalListPageComponent },
          {
            path: 'proposal/:id',
            component: ProposalOverviewPageComponent,
          },
          { path: 'salesorders', component: SalesOrderListPageComponent },
          {
            path: 'salesorder/:id',
            component: SalesOrderOverviewPageComponent,
          },
          { path: 'salesinvoices', component: SalesInvoiceListPageComponent },
          {
            path: 'salesinvoice/:id',
            component: SalesInvoiceOverviewPageComponent,
          },
        ],
      },

      {
        path: 'products',
        children: [
          { path: 'goods', component: GoodListPageComponent },
          {
            path: 'nonunifiedgood/:id',
            component: NonUnifiedGoodOverviewPageComponent,
          },
          { path: 'parts', component: PartListPageComponent },
          {
            path: 'nonunifiedpart/:id',
            component: NonUnifiedPartOverviewPageComponent,
          },
          { path: 'catalogues', component: CataloguesListPageComponent },
          {
            path: 'productcategories',
            component: ProductCategoryListPageComponent,
          },
          {
            path: 'serialiseditemcharacteristics',
            component: SerialisedItemCharacteristicListPageComponent,
          },
          { path: 'brands', component: BrandListPageComponent },
          { path: 'models', component: ModelListPageComponent },
          {
            path: 'producttypes',
            component: ProductTypesOverviewPageComponent,
          },
          {
            path: 'serialiseditems',
            component: SerialisedItemListPageComponent,
          },
          {
            path: 'serialisedItem/:id',
            component: SerialisedItemOverviewPageComponent,
          },
          { path: 'unifiedgoods', component: UnifiedGoodListPageComponent },
          {
            path: 'unifiedgood/:id',
            component: UnifiedGoodOverviewPageComponent,
          },
          { path: 'facilities', component: FacilityListPageComponent },
        ],
      },

      {
        path: 'purchasing',
        children: [
          { path: 'purchaseorders', component: PurchaseOrderListPageComponent },
          {
            path: 'purchaseorder/:id',
            component: PurchaseOrderOverviewPageComponent,
          },
          {
            path: 'purchaseinvoices',
            component: PurchaseInvoiceListPageComponent,
          },
          {
            path: 'purchaseinvoice/:id',
            component: PurchaseInvoiceOverviewPageComponent,
          },
        ],
      },

      {
        path: 'shipment',
        children: [
          { path: 'shipments', component: ShipmentListPageComponent },
          {
            path: 'customershipment/:id',
            component: CustomerShipmentOverviewPageComponent,
          },
          {
            path: 'purchasereturn/:id',
            component: PurchaseReturnOverviewPageComponent,
          },
          {
            path: 'purchaseshipment/:id',
            component: PurchaseShipmentOverviewPageComponent,
          },
          { path: 'carriers', component: CarrierListPageComponent },
        ],
      },

      {
        path: 'workefforts',
        children: [
          {
            path: 'workrequirements',
            component: WorkRequirementListPageComponent,
          },
          {
            path: 'workrequirement/:id',
            component: WorkRequirementOverviewPageComponent,
          },
          { path: 'workefforts', component: WorkEffortListPageComponent },
          { path: 'worktask/:id', component: WorkTaskOverviewPageComponent },
        ],
      },

      {
        path: 'humanresource',
        children: [
          {
            path: 'positiontypes',
            component: PositionTypesListPageComponent,
          },
          {
            path: 'positiontyperates',
            component: PositionTypeRateListPageComponent,
          },
        ],
      },
      {
        path: 'workflow',
        children: [
          {
            path: 'taskassignments',
            component: TaskAssignmentListPageComponent,
          },
        ],
      },
      {
        path: 'accounting',
        children: [
          { path: 'exchangerates', component: ExchangeRateListPageComponent },
        ],
      },
      {
        path: 'admin',
        children: [
          { path: 'emailmessages', component: EmailMessageListPageComponent },
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
  CarrierListPageComponent,
  CataloguesListPageComponent,
  CommunicationEventListPageComponent,
  CustomerShipmentOverviewPageComponent,
  EmailMessageListPageComponent,
  ExchangeRateListPageComponent,
  FacilityListPageComponent,
  GoodListPageComponent,
  NonUnifiedGoodOverviewPageComponent,
  NonUnifiedPartOverviewPageComponent,
  OrganisationListPageComponent,
  OrganisationOverviewPageComponent,
  PartListPageComponent,
  PersonListPageComponent,
  PersonOverviewPageComponent,
  PositionTypeRateListPageComponent,
  PositionTypesListPageComponent,
  ProductCategoryListPageComponent,
  ProductQuoteListPageComponent,
  ProductQuoteOverviewPageComponent,
  ProductTypesOverviewPageComponent,
  PurchaseInvoiceListPageComponent,
  PurchaseInvoiceOverviewPageComponent,
  PurchaseOrderListPageComponent,
  PurchaseOrderOverviewPageComponent,
  PurchaseShipmentOverviewPageComponent,
  RequestForQuoteListPageComponent,
  RequestForQuoteOverviewPageComponent,
  SalesInvoiceListPageComponent,
  SalesInvoiceOverviewPageComponent,
  SalesOrderListPageComponent,
  SalesOrderOverviewPageComponent,
  SerialisedItemCharacteristicListPageComponent,
  SerialisedItemListPageComponent,
  SerialisedItemOverviewPageComponent,
  ShipmentListPageComponent,
  TaskAssignmentListPageComponent,
  UnifiedGoodListPageComponent,
  UnifiedGoodOverviewPageComponent,
  WorkEffortListPageComponent,
  WorkRequirementListPageComponent,
  WorkRequirementOverviewPageComponent,
  WorkTaskOverviewPageComponent,
];
