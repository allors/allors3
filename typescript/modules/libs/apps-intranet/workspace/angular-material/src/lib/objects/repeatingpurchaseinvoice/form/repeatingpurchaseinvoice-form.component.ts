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
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './repeatingpurchaseinvoice-form.component.html',
  providers: [ContextService],
})
export class RepeatingPurchaseInvoiceFormComponent
  implements OnInit, OnDestroy
{
  readonly m: M;

  title: string;
  repeatinginvoice: RepeatingPurchaseInvoice;
  frequencies: TimeFrequency[];
  daysOfWeek: DayOfWeek[];
  supplier: Organisation;
  internalOrganisations: Organisation[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<RepeatingPurchaseInvoiceFormComponent>,
    private errorService: ErrorService,
    private internalOrganisationId: InternalOrganisationId,
    public refreshService: RefreshService
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
          const isCreate = this.data.id == null;
          const id = this.data.id;

          const pulls = [
            pull.Organisation({
              name: 'InternalOrganisations',
              predicate: {
                kind: 'Equals',
                propertyType: m.Organisation.IsInternalOrganisation,
                value: true,
              },
            }),
            pull.TimeFrequency({
              predicate: {
                kind: 'Equals',
                propertyType: m.TimeFrequency.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.TimeFrequency.Name }],
            }),
            pull.DayOfWeek({}),
          ];

          if (!isCreate) {
            pulls.push(
              pull.RepeatingPurchaseInvoice({
                objectId: id,
                include: {
                  Frequency: x,
                  DayOfWeek: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Organisation({ objectId: this.data.associationId })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.supplier = loaded.object<Organisation>(m.Organisation);
        this.repeatinginvoice = loaded.object<RepeatingPurchaseInvoice>(
          m.RepeatingPurchaseInvoice
        );
        this.frequencies = loaded.collection<TimeFrequency>(m.TimeFrequency);
        this.daysOfWeek = loaded.collection<DayOfWeek>(m.DayOfWeek);
        this.internalOrganisations = loaded.collection<Organisation>(
          'InternalOrganisations'
        );

        if (isCreate) {
          this.title = 'Create Repeating Purchase Invoice';
          this.repeatinginvoice =
            this.allors.context.create<RepeatingPurchaseInvoice>(
              m.RepeatingPurchaseInvoice
            );
          this.repeatinginvoice.Supplier = this.supplier;
        } else {
          if (this.repeatinginvoice.canWriteFrequency) {
            this.title = 'Edit Repeating Purchase Invoice';
          } else {
            this.title = 'View Repeating Purchase Invoice';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.repeatinginvoice);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
