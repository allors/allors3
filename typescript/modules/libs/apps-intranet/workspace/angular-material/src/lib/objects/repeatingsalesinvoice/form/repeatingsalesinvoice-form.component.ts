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
  RepeatingSalesInvoice,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './repeatingsalesinvoice-form.component.html',
  providers: [ContextService],
})
export class RepeatingSalesInvoiceFormComponent
  extends AllorsFormComponent<RepeatingSalesInvoice>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  title: string;
  repeatinginvoice: RepeatingSalesInvoice;
  frequencies: TimeFrequency[];
  daysOfWeek: DayOfWeek[];
  invoice: SalesInvoice;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;
          const id = this.data.id;

          const pulls = [
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
              pull.RepeatingSalesInvoice({
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
              pull.SalesInvoice({ objectId: this.data.associationId })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.invoice = loaded.object<SalesInvoice>(m.SalesInvoice);
        this.repeatinginvoice = loaded.object<RepeatingSalesInvoice>(
          m.RepeatingSalesInvoice
        );
        this.frequencies = loaded.collection<TimeFrequency>(m.TimeFrequency);
        this.daysOfWeek = loaded.collection<DayOfWeek>(m.DayOfWeek);

        if (isCreate) {
          this.title = 'Create Repeating Invoice';
          this.repeatinginvoice =
            this.allors.context.create<RepeatingSalesInvoice>(
              m.RepeatingSalesInvoice
            );
          this.repeatinginvoice.Source = this.invoice;
        } else {
          if (this.repeatinginvoice.canWriteFrequency) {
            this.title = 'Edit Repeating Invoice';
          } else {
            this.title = 'View Repeating Invoice';
          }
        }
      });
  }
}
