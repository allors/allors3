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
  Receipt,
  SalesInvoice,
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
  object: SalesInvoice;
  number: string;
  type: string;
  billedTo: string;
  state: string;
  invoiceDate: string;
  dueDate: string;
  description: string;
  currency: string;
  totalExVat: string;
  grandTotal: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './salesinvoice-list-page.component.html',
  providers: [ContextService],
})
export class SalesInvoiceListPageComponent implements OnInit, OnDestroy {
  readonly m: M;

  public title = 'Sales Invoices';

  table: Table<Row>;

  delete: Action;
  print: Action;
  send: Action;
  cancel: Action;
  writeOff: Action;
  copy: Action;
  credit: Action;
  reopen: Action;
  setPaid: Action;

  user: Person;
  internalOrganisation: InternalOrganisation;
  canCreate: boolean;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: ContextService,
    public methodActionService: MethodActionService,
    public printService: PrintService,
    public overviewService: OverviewActionService,
    public deleteService: DeleteActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public refreshService: RefreshService,
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

    this.print = printService.print();
    this.send = methodActionService.create(this.m.SalesInvoice.Send, {
      name: 'Send',
    });
    this.cancel = methodActionService.create(
      this.m.SalesInvoice.CancelInvoice,
      { name: 'Cancel' }
    );
    this.writeOff = methodActionService.create(this.m.SalesInvoice.WriteOff, {
      name: 'WriteOff',
    });
    this.copy = methodActionService.create(this.m.SalesInvoice.Copy, {
      name: 'Copy',
    });
    this.credit = methodActionService.create(this.m.SalesInvoice.Credit, {
      name: 'Credit',
    });
    this.reopen = methodActionService.create(this.m.SalesInvoice.Reopen, {
      name: 'Reopen',
    });

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
          const anyDisabled = (target as SalesInvoice[]).filter(
            (v) => !v.canExecuteSetPaid
          );
          return target.length > 0 ? anyDisabled.length > 0 : true;
        } else {
          return !(target as SalesInvoice).canExecuteSetPaid;
        }
      },
      execute: (target: SalesInvoice) => {
        const invoices = Array.isArray(target)
          ? (target as SalesInvoice[])
          : [target as SalesInvoice];
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
                targets.forEach((salesinvoice) => {
                  const amountToPay =
                    parseFloat(salesinvoice.TotalIncVat) -
                    parseFloat(salesinvoice.AmountPaid);

                  if (
                    salesinvoice.SalesInvoiceType.UniqueId ===
                      '92411bf1-835e-41f8-80af-6611efce5b32' ||
                    salesinvoice.SalesInvoiceType.UniqueId ===
                      'ef5b7c52-e782-416d-b46f-89c8c7a5c24d'
                  ) {
                    const paymentApplication =
                      this.allors.context.create<PaymentApplication>(
                        m.PaymentApplication
                      );
                    paymentApplication.Invoice = salesinvoice;
                    paymentApplication.AmountApplied = amountToPay.toString();

                    // sales invoice
                    if (
                      salesinvoice.SalesInvoiceType.UniqueId ===
                      '92411bf1-835e-41f8-80af-6611efce5b32'
                    ) {
                      const receipt = this.allors.context.create<Receipt>(
                        m.Receipt
                      );
                      receipt.Amount = amountToPay.toString();
                      receipt.EffectiveDate = paymentDate;
                      receipt.Sender = salesinvoice.BilledFrom;
                      receipt.addPaymentApplication(paymentApplication);
                    }

                    // credit note
                    if (
                      salesinvoice.SalesInvoiceType.UniqueId ===
                      'ef5b7c52-e782-416d-b46f-89c8c7a5c24d'
                    ) {
                      const disbursement =
                        this.allors.context.create<Disbursement>(
                          m.Disbursement
                        );
                      disbursement.Amount = amountToPay.toString();
                      disbursement.EffectiveDate = paymentDate;
                      disbursement.Sender = salesinvoice.BilledFrom;
                      disbursement.addPaymentApplication(paymentApplication);
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
        { name: 'billedTo' },
        { name: 'invoiceDate', sort: true },
        { name: 'dueDate', sort: true },
        { name: 'state' },
        { name: 'description', sort: true },
        { name: 'currency' },
        { name: 'totalExVat', sort: true },
        { name: 'grandTotal', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [
        overviewService.overview(),
        this.delete,
        this.print,
        this.cancel,
        this.writeOff,
        this.copy,
        this.credit,
        this.reopen,
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

    this.filter = this.filterService.filter(m.SalesInvoice);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.SalesInvoice.BilledFrom,
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
              pull.SalesInvoice({
                predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.SalesInvoice)?.create(sort)
                  : null,
                include: {
                  PrintDocument: {
                    Media: x,
                  },
                  BillToCustomer: x,
                  SalesInvoiceState: x,
                  SalesInvoiceType: x,
                  DerivedCurrency: x,
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

        this.canCreate = this.internalOrganisation.canExecuteCreateSalesInvoice;

        const salesInvoices = loaded.collection<SalesInvoice>(m.SalesInvoice);
        this.table.total = (loaded.value('SalesInvoices_total') ?? 0) as number;
        this.table.data = salesInvoices
          ?.filter((v) => v.canReadInvoiceNumber)
          ?.map((v) => {
            return {
              object: v,
              number: v.InvoiceNumber,
              type: `${v.SalesInvoiceType && v.SalesInvoiceType.Name}`,
              billedTo: v.BillToCustomer && v.BillToCustomer.DisplayName,
              state: `${v.SalesInvoiceState && v.SalesInvoiceState.Name}`,
              invoiceDate: format(new Date(v.InvoiceDate), 'dd-MM-yyyy'),
              dueDate: `${
                v.DueDate && format(new Date(v.DueDate), 'dd-MM-yyyy')
              }`,
              description: v.Description,
              currency: `${v.DerivedCurrency && v.DerivedCurrency.IsoCode}`,
              totalExVat: v.TotalExVat,
              grandTotal: v.GrandTotal,
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
