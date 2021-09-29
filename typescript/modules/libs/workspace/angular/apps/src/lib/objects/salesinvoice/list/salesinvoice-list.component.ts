import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { format, formatDistance } from 'date-fns';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, InternalOrganisation, SalesInvoice, Disbursement, Receipt, PaymentApplication } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, MethodService, NavigationService, RefreshService, Table, TableRow, TestScope, UserId, OverviewService, ActionTarget, AllorsMaterialDialogService } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { PrintService } from '../../../actions/print/print.service';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { And, Equals } from '@allors/workspace/domain/system';

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
  templateUrl: './salesinvoice-list.component.html',
  providers: [SessionService],
})
export class SalesInvoiceListComponent extends TestScope implements OnInit, OnDestroy {
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
  internalOrganisation: Organisation;
  canCreate: boolean;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: SessionService,

    public methodService: MethodService,
    public printService: PrintService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public refreshService: RefreshService,
    public dialogService: AllorsMaterialDialogService,
    public snackBar: MatSnackBar,
    private internalOrganisationId: InternalOrganisationId,
    private userId: UserId,
    private fetcher: FetcherService,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);
    this.m = this.allors.workspace.configuration.metaPopulation as M;
    const m = this.m;

    this.print = printService.print();
    this.send = methodService.create(allors.client, allors.session, this.m.SalesInvoice.Send, { name: 'Send' });
    this.cancel = methodService.create(allors.client, allors.session, this.m.SalesInvoice.CancelInvoice, { name: 'Cancel' });
    this.writeOff = methodService.create(allors.client, allors.session, this.m.SalesInvoice.WriteOff, { name: 'WriteOff' });
    this.copy = methodService.create(allors.client, allors.session, this.m.SalesInvoice.Copy, { name: 'Copy' });
    this.credit = methodService.create(allors.client, allors.session, this.m.SalesInvoice.Credit, { name: 'Credit' });
    this.reopen = methodService.create(allors.client, allors.session, this.m.SalesInvoice.Reopen, { name: 'Reopen' });

    this.delete = deleteService.delete(allors.client, allors.session);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.setPaid = {
      name: 'setaspaid',
      displayName: () => 'Set as Paid',
      description: () => '',
      disabled: (target: ActionTarget) => {
        if (Array.isArray(target)) {
          const anyDisabled = (target as SalesInvoice[]).filter((v) => !v.canExecuteSetPaid);
          return target.length > 0 ? anyDisabled.length > 0 : true;
        } else {
          return !(target as SalesInvoice).canExecuteSetPaid;
        }
      },
      execute: (target: SalesInvoice) => {
        const invoices = Array.isArray(target) ? (target as SalesInvoice[]) : [target as SalesInvoice];
        const targets = invoices.filter((v) => v.canExecuteSetPaid);

        if (targets.length > 0) {
          dialogService.prompt({ title: `Set Payment Date`, placeholder: `Payment date`, promptType: `date` }).subscribe((paymentDateString: string) => {
            if (paymentDateString) {
              // TODO: Martien
              const paymentDate = new Date(paymentDateString);
              targets.forEach((salesinvoice) => {
                const amountToPay = parseFloat(salesinvoice.TotalIncVat) - parseFloat(salesinvoice.AmountPaid);

                if (salesinvoice.SalesInvoiceType.UniqueId === '92411bf1-835e-41f8-80af-6611efce5b32' || salesinvoice.SalesInvoiceType.UniqueId === 'ef5b7c52-e782-416d-b46f-89c8c7a5c24d') {
                  const paymentApplication = this.allors.session.create<PaymentApplication>(m.PaymentApplication);
                  paymentApplication.Invoice = salesinvoice;
                  paymentApplication.AmountApplied = amountToPay.toString();

                  // sales invoice
                  if (salesinvoice.SalesInvoiceType.UniqueId === '92411bf1-835e-41f8-80af-6611efce5b32') {
                    const receipt = this.allors.session.create<Receipt>(m.Receipt);
                    receipt.Amount = amountToPay.toString();
                    receipt.EffectiveDate = paymentDate;
                    receipt.Sender = salesinvoice.BilledFrom;
                    receipt.addPaymentApplication(paymentApplication);
                  }

                  // credit note
                  if (salesinvoice.SalesInvoiceType.UniqueId === 'ef5b7c52-e782-416d-b46f-89c8c7a5c24d') {
                    const disbursement = this.allors.session.create<Disbursement>(m.Disbursement);
                    disbursement.Amount = amountToPay.toString();
                    disbursement.EffectiveDate = paymentDate;
                    disbursement.Sender = salesinvoice.BilledFrom;
                    disbursement.addPaymentApplication(paymentApplication);
                  }
                }
              });

              this.allors.client.pushReactive(this.allors.session).subscribe(() => {
                snackBar.open('Successfully set to fully paid.', 'close', { duration: 5000 });
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
      actions: [overviewService.overview(), this.delete, this.print, this.cancel, this.writeOff, this.copy, this.credit, this.reopen, this.setPaid],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'number',
      initialSortDirection: 'desc',
    });
  }
  public ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    const angularMeta = this.allors.workspace.services.angularMetaService;
    const angularSalesInvoice = angularMeta.for(m.SalesInvoice);
    this.filter = angularSalesInvoice.filter ??= new Filter(angularSalesInvoice.filterDefinition);

    const internalOrganisationPredicate: Equals = { kind: 'Equals', propertyType: m.SalesInvoice.BilledFrom };
    const predicate: And = { kind: 'And', operands: [internalOrganisationPredicate, this.filter.definition.predicate] };

    this.subscription = combineLatest([this.refreshService.refresh$, this.filter.fields$, this.table.sort$, this.table.pager$, this.internalOrganisationId.observable$])
      .pipe(
        scan(([previousRefresh, previousFilterFields], [refresh, filterFields, sort, pageEvent, internalOrganisationId]) => {
          pageEvent =
            previousRefresh !== refresh || filterFields !== previousFilterFields
              ? {
                  ...pageEvent,
                  pageIndex: 0,
                }
              : pageEvent;

          if (pageEvent.pageIndex === 0) {
            this.table.pageIndex = 0;
          }

          return [refresh, filterFields, sort, pageEvent, internalOrganisationId];
        }),
        switchMap(([, filterFields, sort, pageEvent, internalOrganisationId]) => {
          internalOrganisationPredicate.value = internalOrganisationId;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Person({
              objectId: this.userId.value,
            }),
            pull.SalesInvoice({
              predicate,
              sorting: sort ? angularSalesInvoice.sorter?.create(sort) : null,
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

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        this.internalOrganisation = loaded.object<Organisation>(m.InternalOrganisation);
        this.user = loaded.object<Person>(m.Person);

        this.canCreate = this.internalOrganisation.canExecuteCreateSalesInvoice;

        const salesInvoices = loaded.collection<SalesInvoice>(m.SalesInvoice);
        this.table.total = loaded.value('SalesInvoices_total') as number;
        this.table.data = salesInvoices
          .filter((v) => v.canReadInvoiceNumber)
          .map((v) => {
            return {
              object: v,
              number: v.InvoiceNumber,
              type: `${v.SalesInvoiceType && v.SalesInvoiceType.Name}`,
              billedTo: v.BillToCustomer && displayName(v.BillToCustomer),
              state: `${v.SalesInvoiceState && v.SalesInvoiceState.Name}`,
              invoiceDate: format(new Date(v.InvoiceDate), 'dd-MM-yyyy'),
              dueDate: `${v.DueDate && format(new Date(v.DueDate), 'dd-MM-yyyy')}`,
              description: v.Description,
              currency: `${v.DerivedCurrency && v.DerivedCurrency.IsoCode}`,
              totalExVat: v.TotalExVat,
              grandTotal: v.GrandTotal,
              lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
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
