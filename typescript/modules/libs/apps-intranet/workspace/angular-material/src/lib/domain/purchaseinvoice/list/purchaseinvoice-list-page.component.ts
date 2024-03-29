import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import { And, Equals } from '@allors/system/workspace/domain';
import {
  Disbursement,
  InternalOrganisation,
  PaymentApplication,
  Person,
  PurchaseInvoice,
  Receipt,
} from '@allors/default/workspace/domain';
import {
  Action,
  ActionTarget,
  AllorsDialogService,
  Filter,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  Table,
  TableRow,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  DeleteActionService,
  MethodActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { PrintService } from '../../../actions/print/print.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { format, formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: PurchaseInvoice;
  number: string;
  type: string;
  billedFrom: string;
  state: string;
  reference: string;
  dueDate: string;
  currency: string;
  totalExVat: string;
  totalInvoice: string;
  lastModifiedDate: string;
}
@Component({
  templateUrl: './purchaseinvoice-list-page.component.html',
  providers: [ContextService],
})
export class PurchaseInvoiceListPageComponent implements OnInit, OnDestroy {
  readonly m: M;

  public title = 'Purchase Invoices';

  table: Table<Row>;

  delete: Action;
  approve: Action;
  cancel: Action;
  reopen: Action;
  reject: Action;
  print: Action;
  setPaid: Action;

  user: Person;
  internalOrganisation: InternalOrganisation;
  canCreate: boolean;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: ContextService,

    public refreshService: RefreshService,
    public navigation: NavigationService,
    public methodActionService: MethodActionService,
    public printService: PrintService,
    public deleteService: DeleteActionService,
    public overviewService: OverviewActionService,
    public mediaService: MediaService,
    public dialogService: AllorsDialogService,
    public snackBar: MatSnackBar,
    private internalOrganisationId: InternalOrganisationId,
    private userId: UserId,
    private fetcher: FetcherService,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;
    const m = this.m;

    this.approve = methodActionService.create(this.m.PurchaseInvoice.Approve, {
      name: 'Approve',
    });
    this.reject = methodActionService.create(this.m.PurchaseInvoice.Reject, {
      name: 'Reject',
    });
    this.cancel = methodActionService.create(this.m.PurchaseInvoice.Cancel, {
      name: 'Cancel',
    });
    this.reopen = methodActionService.create(this.m.PurchaseInvoice.Reopen, {
      name: 'Reopen',
    });
    this.print = printService.print();

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.setPaid = {
      name: 'setaspaid',
      displayName: () => 'Set as Paid',
      description: () => '',
      disabled: (target: ActionTarget) => {
        if (Array.isArray(target)) {
          const anyDisabled = (target as PurchaseInvoice[]).filter(
            (v) => !v.canExecuteSetPaid
          );
          return target.length > 0 ? anyDisabled.length > 0 : true;
        } else {
          return !(target as PurchaseInvoice).canExecuteSetPaid;
        }
      },
      execute: (target: PurchaseInvoice) => {
        const invoices = Array.isArray(target)
          ? (target as PurchaseInvoice[])
          : [target as PurchaseInvoice];
        const targets = invoices.filter((v) => v.canExecuteSetPaid);

        if (targets.length > 0) {
          dialogService
            .prompt({
              title: `Set Payment Date`,
              placeholder: `Payment date`,
              promptType: `date`,
            })
            .subscribe((paymentDateString: string) => {
              if (paymentDateString) {
                // TODO: Martien
                const paymentDate = new Date(paymentDateString);
                targets.forEach((purchaseInvoice) => {
                  const amountToPay =
                    parseFloat(purchaseInvoice.TotalIncVat) -
                    parseFloat(purchaseInvoice.AmountPaid);

                  if (
                    purchaseInvoice.PurchaseInvoiceType.UniqueId ===
                      'd08f0309-a4cb-4ab7-8f75-3bb11dcf3783' ||
                    purchaseInvoice.PurchaseInvoiceType.UniqueId ===
                      '0187d927-81f5-4d6a-9847-58b674ad3e6a'
                  ) {
                    const paymentApplication =
                      this.allors.context.create<PaymentApplication>(
                        m.PaymentApplication
                      );
                    paymentApplication.Invoice = purchaseInvoice;
                    paymentApplication.AmountApplied = amountToPay.toString();

                    // purchase invoice
                    if (
                      purchaseInvoice.PurchaseInvoiceType.UniqueId ===
                      'd08f0309-a4cb-4ab7-8f75-3bb11dcf3783'
                    ) {
                      const disbursement =
                        this.allors.context.create<Disbursement>(
                          m.Disbursement
                        );
                      disbursement.Amount = amountToPay.toString();
                      disbursement.EffectiveDate = paymentDate;
                      disbursement.Sender = purchaseInvoice.BilledFrom;
                      disbursement.addPaymentApplication(paymentApplication);
                    }

                    // purchase return
                    if (
                      purchaseInvoice.PurchaseInvoiceType.UniqueId ===
                      '0187d927-81f5-4d6a-9847-58b674ad3e6a'
                    ) {
                      const receipt = this.allors.context.create<Receipt>(
                        m.Receipt
                      );
                      receipt.Amount = amountToPay.toString();
                      receipt.EffectiveDate = paymentDate;
                      receipt.Sender = purchaseInvoice.BilledFrom;
                      receipt.addPaymentApplication(paymentApplication);
                    }
                  }
                });

                this.allors.context.push().subscribe(() => {
                  snackBar.open('Successfully set to fully paid.', 'close', {
                    duration: 5000,
                  });
                  refreshService.refresh();
                });
              }
            });
        }
      },
      result: null,
    };

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort: true },
        { name: 'type', sort: true },
        { name: 'billedFrom' },
        { name: 'state' },
        { name: 'reference', sort: true },
        { name: 'dueDate', sort: true },
        { name: 'currency' },
        { name: 'totalExVat', sort: true },
        { name: 'totalInvoice', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [
        overviewService.overview(),
        this.delete,
        this.approve,
        this.cancel,
        this.reopen,
        this.reject,
        this.print,
        this.setPaid,
      ],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'number',
      initialSortDirection: 'desc',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.PurchaseInvoice);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.PurchaseInvoice.BilledTo,
    };

    const predicate: And = {
      kind: 'And',
      operands: [
        internalOrganisationPredicate,
        this.filter.definition.predicate,
      ],
    };

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        scan(
          (
            [previousRefresh, previousFilterFields],
            [refresh, filterFields, sort, pageEvent, internalOrganisationId]
          ) => {
            pageEvent =
              previousRefresh !== refresh ||
              filterFields !== previousFilterFields
                ? {
                    ...pageEvent,
                    pageIndex: 0,
                  }
                : pageEvent;

            if (pageEvent.pageIndex === 0) {
              this.table.pageIndex = 0;
            }

            return [
              refresh,
              filterFields,
              sort,
              pageEvent,
              internalOrganisationId,
            ];
          }
        ),
        switchMap(
          ([, filterFields, sort, pageEvent, internalOrganisationId]: [
            Date,
            FilterField[],
            Sort,
            PageEvent,
            number
          ]) => {
            internalOrganisationPredicate.value = internalOrganisationId;

            const pulls = [
              this.fetcher.internalOrganisation,
              pull.Person({
                objectId: this.userId.value,
              }),
              pull.PurchaseInvoice({
                predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.PurchaseInvoice)?.create(sort)
                  : null,
                include: {
                  BilledFrom: x,
                  BilledTo: x,
                  PurchaseInvoiceState: x,
                  PurchaseInvoiceType: x,
                  DerivedCurrency: x,
                  PrintDocument: {
                    Media: x,
                  },
                },
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
            ];

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.user = loaded.object<Person>(m.Person);

        this.canCreate =
          this.internalOrganisation.canExecuteCreatePurchaseInvoice;

        const purchaseInvoices = loaded.collection<PurchaseInvoice>(
          m.PurchaseInvoice
        );
        this.table.data = purchaseInvoices
          ?.filter((v) => v.canReadInvoiceNumber)
          ?.map((v) => {
            return {
              object: v,
              number: v.InvoiceNumber,
              type: `${v.PurchaseInvoiceType && v.PurchaseInvoiceType.Name}`,
              billedFrom: v.BilledFrom && v.BilledFrom.DisplayName,
              state: `${v.PurchaseInvoiceState && v.PurchaseInvoiceState.Name}`,
              reference: `${v.CustomerReference}`,
              dueDate: v.DueDate && format(new Date(v.DueDate), 'dd-MM-yyyy'),
              currency: `${v.DerivedCurrency && v.DerivedCurrency.IsoCode}`,
              totalExVat: v.TotalExVat,
              totalInvoice: v.GrandTotal,
              lastModifiedDate: formatDistance(
                new Date(v.LastModifiedDate),
                new Date()
              ),
            } as Row;
          });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
