import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation, TimeFrequency, DayOfWeek, RepeatingPurchaseInvoice } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './repeatingpurchaseinvoice-edit.component.html',
  providers: [ContextService],
})
export class RepeatingPurchaseInvoiceEditComponent extends TestScope implements OnInit, OnDestroy {
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
    public dialogRef: MatDialogRef<RepeatingPurchaseInvoiceEditComponent>,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId,
    public refreshService: RefreshService
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;
          const id = this.data.id;

          const pulls = [
            pull.Organisation({
              name: 'InternalOrganisations',
              predicate: { kind: 'Equals', propertyType: m.Organisation.IsInternalOrganisation, value: true },
            }),
            pull.TimeFrequency({
              predicate: { kind: 'Equals', propertyType: m.TimeFrequency.IsActive, value: true },
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
            pulls.push(pull.Organisation({ objectId: this.data.associationId }));
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.supplier = loaded.object<Organisation>(m.Organisation);
        this.repeatinginvoice = loaded.object<RepeatingPurchaseInvoice>(m.RepeatingPurchaseInvoice);
        this.frequencies = loaded.collection<TimeFrequency>(m.TimeFrequency);
        this.daysOfWeek = loaded.collection<DayOfWeek>(m.DayOfWeek);
        this.internalOrganisations = loaded.collection<Organisation>(m.Organisation);

        if (isCreate) {
          this.title = 'Create Repeating Purchase Invoice';
          this.repeatinginvoice = this.allors.context.create<RepeatingPurchaseInvoice>(m.RepeatingPurchaseInvoice);
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
    }, this.saveService.errorHandler);
  }
}
