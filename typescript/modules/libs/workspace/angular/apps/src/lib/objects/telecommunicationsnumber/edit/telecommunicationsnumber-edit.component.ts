import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { ContactMechanism, Enumeration, TelecommunicationsNumber } from '@allors/workspace/domain/default';
import { RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './telecommunicationsnumber-edit.component.html',
  providers: [ContextService],
})
export class TelecommunicationsNumberEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  contactMechanism: TelecommunicationsNumber;
  contactMechanismTypes: Enumeration[];
  title: string;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: IObject,
    public dialogRef: MatDialogRef<TelecommunicationsNumberEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.ContactMechanism({
              objectId: this.data.id,
            }),
            pull.ContactMechanismType({
              predicate: { kind: 'Equals', propertyType: m.ContactMechanismType.IsActive, value: true },
              sorting: [{ roleType: this.m.ContactMechanismType.Name }],
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.contactMechanismTypes = loaded.collection<Enumeration>(m.Enumeration);
        this.contactMechanism = loaded.object<TelecommunicationsNumber>(m.ContactMechanism);

        if (this.contactMechanism.canWriteAreaCode) {
          this.title = 'Edit Phone Number';
        } else {
          this.title = 'View Phone Number';
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
      this.dialogRef.close(this.contactMechanism);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
