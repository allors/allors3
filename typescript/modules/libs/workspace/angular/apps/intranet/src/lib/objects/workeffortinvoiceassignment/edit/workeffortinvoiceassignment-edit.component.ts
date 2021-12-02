import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { ObjectData, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { InternalOrganisation, InvoiceItemType, WorkEffortInvoiceItem, WorkEffort, WorkEffortInvoiceItemAssignment } from '@allors/workspace/domain/default';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './workeffortinvoiceassignment-edit.component.html',
  providers: [ContextService],
})
export class WorkEffortInvoiceItemAssignmentEditComponent implements OnInit, OnDestroy {
  readonly m: M;

  title: string;
  workEffortInvoiceItemAssignment: WorkEffortInvoiceItemAssignment;
  workEffort: WorkEffort;
  internalOrganisation: InternalOrganisation;

  private subscription: Subscription;
  workEffortInvoiceItem: WorkEffortInvoiceItem;
  invoiceItemTypes: InvoiceItemType[];

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkEffortInvoiceItemAssignmentEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private snackBar: MatSnackBar,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.InvoiceItemType({
              predicate: { kind: 'Equals', propertyType: m.InvoiceItemType.IsActive, value: true },
              sorting: [{ roleType: m.InvoiceItemType.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.WorkEffortInvoiceItemAssignment({
                objectId: this.data.id,
                include: {
                  WorkEffortInvoiceItem: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.WorkEffort({
                objectId: this.data.associationId,
              })
            );
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
        this.invoiceItemTypes = loaded.collection<InvoiceItemType>(m.InvoiceItemType);

        if (isCreate) {
          this.title = 'Add work effort invoice item';

          this.workEffortInvoiceItemAssignment = this.allors.context.create<WorkEffortInvoiceItemAssignment>(m.WorkEffortInvoiceItemAssignment);
          this.workEffortInvoiceItemAssignment.Assignment = this.workEffort;
          this.workEffortInvoiceItem = this.allors.context.create<WorkEffortInvoiceItem>(m.WorkEffortInvoiceItem);
          this.workEffortInvoiceItemAssignment.WorkEffortInvoiceItem = this.workEffortInvoiceItem;
        } else {
          this.workEffortInvoiceItemAssignment = loaded.object<WorkEffortInvoiceItemAssignment>(m.WorkEffortInvoiceItemAssignment);
          this.workEffortInvoiceItem = this.workEffortInvoiceItemAssignment.WorkEffortInvoiceItem;

          if (this.workEffortInvoiceItemAssignment.canWriteWorkEffortInvoiceItem) {
            this.title = 'Edit invoice item';
          } else {
            this.title = 'View invoice item';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public update(): void {
    this.allors.context.push().subscribe(() => {
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.workEffortInvoiceItemAssignment);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
