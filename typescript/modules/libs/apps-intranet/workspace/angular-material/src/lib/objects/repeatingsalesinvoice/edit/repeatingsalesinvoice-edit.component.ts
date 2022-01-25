import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  TimeFrequency,
  SalesInvoice,
  RepeatingSalesInvoice,
  DayOfWeek,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

@Component({
  templateUrl: './repeatingsalesinvoice-edit.component.html',
  providers: [ContextService],
})
export class RepeatingSalesInvoiceEditComponent implements OnInit, OnDestroy {
  readonly m: M;

  title: string;
  repeatinginvoice: RepeatingSalesInvoice;
  frequencies: TimeFrequency[];
  daysOfWeek: DayOfWeek[];
  invoice: SalesInvoice;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<RepeatingSalesInvoiceEditComponent>,
    private errorService: ErrorService,
    public refreshService: RefreshService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
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
