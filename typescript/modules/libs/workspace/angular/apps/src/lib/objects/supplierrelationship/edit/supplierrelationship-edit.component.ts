import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation, InternalOrganisation, SupplierRelationship } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './supplierrelationship-edit.component.html',
  providers: [ContextService],
})
export class SupplierRelationshipEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  partyRelationship: SupplierRelationship;
  internalOrganisation: Organisation;
  organisation: Organisation;
  title: string;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SupplierRelationshipEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public canCreate(createData: ObjectData) {
    if (createData.associationObjectType === this.m.Person) {
      return false;
    }

    return true;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [this.fetcher.internalOrganisation];

          if (!isCreate) {
            pulls.push(
              pull.SupplierRelationship({
                objectId: this.data.id,
                include: {
                  InternalOrganisation: x,
                  Parties: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Organisation({
                objectId: this.data.associationId,
              })
            );
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation = loaded.object<Organisation>(m.InternalOrganisation);
        this.organisation = loaded.object<Organisation>(m.Organisation);

        if (isCreate) {
          if (this.organisation === undefined) {
            this.dialogRef.close();
          }

          this.title = 'Add Supplier Relationship';

          this.partyRelationship = this.allors.context.create<SupplierRelationship>(m.SupplierRelationship);
          this.partyRelationship.FromDate = new Date();
          this.partyRelationship.Supplier = this.organisation;
          this.partyRelationship.InternalOrganisation = this.internalOrganisation;
          this.partyRelationship.NeedsApproval = false;
        } else {
          this.partyRelationship = loaded.object<SupplierRelationship>(m.SupplierRelationship);

          if (this.partyRelationship.canWriteFromDate) {
            this.title = 'Edit Supplier Relationship';
          } else {
            this.title = 'View Supplier Relationship';
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
      this.dialogRef.close(this.partyRelationship);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
