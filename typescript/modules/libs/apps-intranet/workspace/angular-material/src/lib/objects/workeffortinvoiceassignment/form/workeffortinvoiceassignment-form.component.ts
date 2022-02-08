import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './workeffortinvoiceassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortInvoiceItemAssignmentFormComponent
  implements OnInit, OnDestroy
{
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
    public dialogRef: MatDialogRef<WorkEffortInvoiceItemAssignmentFormComponent>,
    public refreshService: RefreshService,
    private errorService: ErrorService,
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

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.InvoiceItemType({
              predicate: {
                kind: 'Equals',
                propertyType: m.InvoiceItemType.IsActive,
                value: true,
              },
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

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
        this.invoiceItemTypes = loaded.collection<InvoiceItemType>(
          m.InvoiceItemType
        );

        if (isCreate) {
          this.title = 'Add work effort invoice item';

          this.workEffortInvoiceItemAssignment =
            this.allors.context.create<WorkEffortInvoiceItemAssignment>(
              m.WorkEffortInvoiceItemAssignment
            );
          this.workEffortInvoiceItemAssignment.Assignment = this.workEffort;

          this.workEffortInvoiceItem =
            this.allors.context.create<WorkEffortInvoiceItem>(
              m.WorkEffortInvoiceItem
            );
          this.workEffortInvoiceItemAssignment.WorkEffortInvoiceItem =
            this.workEffortInvoiceItem;
        } else {
          this.workEffortInvoiceItemAssignment =
            loaded.object<WorkEffortInvoiceItemAssignment>(
              m.WorkEffortInvoiceItemAssignment
            );
          this.workEffortInvoiceItem =
            this.workEffortInvoiceItemAssignment.WorkEffortInvoiceItem;

          if (
            this.workEffortInvoiceItemAssignment.canWriteWorkEffortInvoiceItem
          ) {
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
    }, this.errorService.errorHandler);
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.workEffortInvoiceItemAssignment);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
