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
  templateUrl: './telecommunicationsnumber-form.component.html',
  providers: [ContextService],
})
export class TelecommunicationsNumberFormComponent
  implements OnInit, OnDestroy
{
  readonly m: M;

  contactMechanism: TelecommunicationsNumber;
  contactMechanismTypes: Enumeration[];
  title: string;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: IObject,
    public dialogRef: MatDialogRef<TelecommunicationsNumberFormComponent>,
    public refreshService: RefreshService,
    private errorService: ErrorService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.ContactMechanism({
              objectId: this.data.id,
            }),
            pull.ContactMechanismType({
              predicate: {
                kind: 'Equals',
                propertyType: m.ContactMechanismType.IsActive,
                value: true,
              },
              sorting: [{ roleType: this.m.ContactMechanismType.Name }],
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.contactMechanismTypes = loaded.collection<ContactMechanismType>(
          m.ContactMechanismType
        );
        this.contactMechanism = loaded.object<TelecommunicationsNumber>(
          m.ContactMechanism
        );

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
    }, this.errorService.errorHandler);
  }
}
