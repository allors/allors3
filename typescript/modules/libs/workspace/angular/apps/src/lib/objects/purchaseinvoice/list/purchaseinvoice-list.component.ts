import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { format, formatDistance } from 'date-fns';
import { MatSnackBar } from '@angular/material/snack-bar/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, InternalOrganisation, Receipt, PaymentApplication, PurchaseInvoice } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, MethodService, NavigationService, RefreshService, Table, TableRow, TestScope, UserId, OverviewService, ActionTarget, AllorsMaterialDialogService } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { PrintService } from '../../../actions/print/print.service';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Equals } from '@allors/workspace/domain/system';

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
  templateUrl: './purchaseinvoice-list.component.html',
  providers: [SessionService],
})
export class PurchaseInvoiceListComponent extends TestScope implements OnInit, OnDestroy {
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
  internalOrganisation: Organisation;
  canCreate: boolean;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: SessionService,

    public refreshService: RefreshService,
    public navigation: NavigationService,
    public methodService: MethodService,
    public printService: PrintService,
    public deleteService: DeleteService,
    public overviewService: OverviewService,
    public mediaService: MediaService,
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

    this.approve = methodService.create(allors.client, allors.session, this.m.PurchaseInvoice.Approve, { name: 'Approve' });
    this.reject = methodService.create(allors.client, allors.session, this.m.PurchaseInvoice.Reject, { name: 'Reject' });
    this.cancel = methodService.create(allors.client, allors.session, this.m.PurchaseInvoice.Cancel, { name: 'Cancel' });
    this.reopen = methodService.create(allors.client, allors.session, this.m.PurchaseInvoice.Reopen, { name: 'Reopen' });
    this.print = printService.print();

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
          const anyDisabled = (target as PurchaseInvoice[]).filter((v) => !v.canExecuteSetPaid);
          return target.length > 0 ? anyDisabled.length > 0 : true;
        } else {
          return !(target as PurchaseInvoice).canExecuteSetPaid;
        }
      },
      execute: (target: PurchaseInvoice) => {
        const invoices = Array.isArray(target) ? (target as PurchaseInvoice[]) : [target as PurchaseInvoice];
        const targets = invoices.filter((v) => v.canExecuteSetPaid);

        if (targets.length > 0) {
          dialogService.prompt({ title: `Set Payment Date`, placeholder: `Payment date`, promptType: `date` }).subscribe((paymentDate: string) => {
            if (paymentDate) {
              targets.forEach((purchaseInvoice) => {
                const amountToPay = parseFloat(purchaseInvoice.TotalIncVat) - parseFloat(purchaseInvoice.AmountPaid);

                if (purchaseInvoice.PurchaseInvoiceType.UniqueId === 'd08f0309-a4cb-4ab7-8f75-3bb11dcf3783' || purchaseInvoice.PurchaseInvoiceType.UniqueId === '0187d927-81f5-4d6a-9847-58b674ad3e6a') {
                  const paymentApplication = this.allors.session.create<PaymentApplication>(m.PaymentApplication);
                  paymentApplication.Invoice = purchaseInvoice;
                  paymentApplication.AmountApplied = amountToPay.toString();

                  // purchase invoice
                  if (purchaseInvoice.PurchaseInvoiceType.UniqueId === 'd08f0309-a4cb-4ab7-8f75-3bb11dcf3783') {
                    const disbursement = this.allors.session.create<Disbursement>(m.Disbursement);
                    disbursement.Amount = amountToPay.toString();
                    disbursement.EffectiveDate = paymentDate;
                    disbursement.Sender = purchaseInvoice.BilledFrom;
                    disbursement.addPaymentApplication(paymentApplication);
                  }

                  // purchase return
                  if (purchaseInvoice.PurchaseInvoiceType.UniqueId === '0187d927-81f5-4d6a-9847-58b674ad3e6a') {
                    const receipt = this.allors.session.create<Receipt>(m.Receipt);
                    receipt.Amount = amountToPay.toString();
                    receipt.EffectiveDate = paymentDate;
                    receipt.Sender = purchaseInvoice.BilledFrom;
                    receipt.addPaymentApplication(paymentApplication);
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
        { name: 'billedFrom' },
        { name: 'state' },
        { name: 'reference', sort: true },
        { name: 'dueDate', sort: true },
        { name: 'currency' },
        { name: 'totalExVat', sort: true },
        { name: 'totalInvoice', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.delete, this.approve, this.cancel, this.reopen, this.reject, this.print, this.setPaid],
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
    this.filter = m.PurchaseInvoice.filter = m.PurchaseInvoice.filter ?? new Filter(m.PurchaseInvoice.filterDefinition);

    const internalOrganisationPredicate : Equals = { kind: 'Equals', propertyType: m.PurchaseInvoice.BilledTo };

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
          internalOrganisationPredicate.object = internalOrganisationId;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Person({
              object: this.userId.value,
            }),
            pull.PurchaseInvoice({
              predicate,
              sorting: sort ? m.PurchaseInvoice.sorter.create(sort) : null,
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

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        this.internalOrganisation = loaded.object<Organisation>(m.InternalOrganisation);
        this.user = loaded.object<Person>(m.Person);

        this.canCreate = this.internalOrganisation.canExecuteCreatePurchaseInvoice;

        const purchaseInvoices = loaded.collection<PurchaseInvoice>(m.PurchaseInvoice);
        this.table.total = loaded.value('PurchaseInvoices_total') as number;
        this.table.data = purchaseInvoices
          .filter((v) => v.canReadInvoiceNumber)
          .map((v) => {
            return {
              object: v,
              number: v.InvoiceNumber,
              type: `${v.PurchaseInvoiceType && v.PurchaseInvoiceType.Name}`,
              billedFrom: v.BilledFrom && v.BilledFrom.displayName,
              state: `${v.PurchaseInvoiceState && v.PurchaseInvoiceState.Name}`,
              reference: `${v.CustomerReference}`,
              dueDate: v.DueDate && format(new Date(v.DueDate), 'dd-MM-yyyy'),
              currency: `${v.DerivedCurrency && v.DerivedCurrency.IsoCode}`,
              totalExVat: v.TotalExVat,
              totalInvoice: v.GrandTotal,
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
